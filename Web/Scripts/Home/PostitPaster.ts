﻿//感情のリスト[surprised,anger,fun,bethink,good,sad,noidea]
interface EventTarget
{
    src: string;
}
interface ParagraphEachHandler
{
    (emotion:string,count:number,itr:number):void;
}

interface ParagraphCallHandler
{
    ():void;
}

class  CountWholeEmotionsResult
{
    surprised: number=0;
    anger: number=0;
    fun: number=0;
    bethink: number=0;
    good: number=0;
    sad: number=0;
    noidea: number=0;
    sum:number=0;
}

interface ILabelSourceParser
{
    getLabelCount(paragraphId: string, emotion: string): number;

    eachByParagraph(paragraphId: string, hander: ParagraphEachHandler): void;

    callByParagraph(paragraphId: string, handler: ParagraphCallHandler): void;

    countWholeEmotions():CountWholeEmotionsResult;
}

class LabelSourceParser implements ILabelSourceParser
{
    private jsonSource:any;
    constructor()
    {
        this.jsonSource = JSON.parse($("#label-info").text());
    }

    getLabelCount(paragraphId: string, emotion: string): number
    {
        for (var i = 0; i < this.jsonSource.length; i++)
        {
            if (this.jsonSource[i]["ParagraphId"] == paragraphId || "p-"+this.jsonSource[i]["ParagraphId"] == paragraphId)
            {
                var data = JSON.parse(this.jsonSource[i]["Data"]);
                for (var j = 0; j < data.length; j++)
                {
                    if (data[j].Key == emotion)
                    {
                        return data[j].Value;
                    }
                }
                return 0;
            }
        }
        return 0;
    }

    eachByParagraph(paragraphId: string, handler: ParagraphEachHandler): void
    {
        for (var i = 0; i < this.jsonSource.length; i++) {
            if (this.jsonSource[i].ParagraphId == paragraphId || "p-"+this.jsonSource[i].ParagraphId == paragraphId) {
                var data = JSON.parse(this.jsonSource[i]["Data"]);
                data = _.sortBy(data, d => (Object)(d).Value).reverse();
                for (var j = 0; j < data.length; j++)
                {
                    handler(data[j].Key, data[j].Value, j);
                }
                return null;
            }
        }
        return null;
    }

    callByParagraph(paragraphId: string, handler: ParagraphCallHandler): void
    {
        for (var i = 0; i < this.jsonSource.length; i++) {
            if (this.jsonSource[i]["ParagraphId"] == paragraphId)
            {
                handler();
                return null;
            }
        }
        return null;
    }

    countWholeEmotions(): CountWholeEmotionsResult
    {
        var result: CountWholeEmotionsResult = new CountWholeEmotionsResult();
        for (var i = 0; i < this.jsonSource.length; i++) {
                var data = JSON.parse(this.jsonSource[i]["Data"]);
            for (var j = 0; j < data.length; j++)
            {
                result[data[j].Key] += data[j].Value;
                result["sum"] += data[j].Value;
            }
        }
        return result;
    }
}

var labelSourceParser: ILabelSourceParser;
    labelSourceParser = new LabelSourceParser();


interface ILabelBoxController
{
    labelPosition(speciesOfLabel: number, boxClass: string): void;
}

class LabelBoxController implements ILabelBoxController {
    labelPosition(speciesOfLabel: number, boxClass: string) {
        var boxSelector: string = ".dropbox ." + boxClass;
        if (43 * speciesOfLabel >= $(boxSelector).height()) {
            var sortArray: Element[] = [];
            $(boxSelector + ' > [class]').each((i, element) => {
                sortArray[sortArray.length] = element;
            });
            sortArray.reverse();

            $(boxSelector + ' > *').css({
                "margin-right": "-53px",
                "float": "left"
            });
            $(boxSelector + ':after').css({
                "content": "''",
                "display": "block",
                "clear": "both"
            });

            for (var i: number = 0, len: number = sortArray.length; i < len; i++) {
                $(boxSelector).append(sortArray[i]);
            }
        }
    }

}
var labelBoxController = new LabelBoxController();

class AjaxManager
{
    sendPostitNumber(articleId: string, thisClass: string, labelType: string, postitExistence: any, $target: JQuery, src: string) {
        $.ajax({
            type: "post",
            url: "api/Label/AttachLabel",
            data: {
                "ArticleId": articleId,
                "ParagraphId": getParagraphId(thisClass),
                "LabelType": labelType
            },
            success: (data) => {
                if (data.isSucceed)
                {
                    wholeCount[labelType]++;//ドーナツ用のデータの更新
                    updateArticleDounught(false);//更新したデータの適用
                    var updatedCount: any = $(".label-count").text();
                    $(".label-count").text(parseInt(updatedCount)+1);
                    if (postitExistence) {
                        labelSourceParser.callByParagraph(thisClass.substr(4), () => {
                            $('.dropbox > .' + thisClass + ' > .' + labelType + ' > span').html(String(
                                Number($('.dropbox > .' + thisClass + ' > .' + labelType + ' > span').text()) + 1
                                ));
                        });
                    } else {
                        $target.append(
                            '<div class="' + labelType + '" style="background-image:url(\'Content/imgs/Home/' + labelType + '-d.svg\');background-size:130px 43px;height:43px;width:130px;"><span>1</span></div>'
                            );
                    }
                    labelBoxController.labelPosition($('.dropbox > .' + thisClass + ' > *').length, thisClass);
                } else {
                    $().alertwindow("１つの段落に２つ以上のふせんをつける事はできません", "ok"); // jquery.alertwindow.js
                }
            }
        });
    }
}


var ajaxManager = new AjaxManager();


$(window).load(() => // 後読みじゃないとまともにポジションとれない
{

    var htmlHeight = ($('.foot').offset()||{top:NaN}).top + $('.foot').outerHeight();

    $('html').css({
        "height": htmlHeight + "px"
    });
    var articleId = location.pathname.substr(1);

    var pasteMode: boolean = false;

    var labelType: string, src: string;

    var dropboxPosX: number = $('.dropbox').offset().left;

    var dropboxPosY: number = $('.contentswrapper').offset().top,
        dropboxHeight: number = $('.contentswrapper').outerHeight(true);

    var posY: number = dropboxPosY;

    $('.article-container > *').each((i) => {
        var $ele: JQuery = $('.article-container > [class*="p-"]:nth-child(' + (i + 1) + ')');
        if (!$ele[0]) return true;
        var elementName: string = $ele[0].tagName;
        if (elementName == "hr") return true;
        

        var className = $ele.attr("class");

        var eleHeight: number = $ele.outerHeight(true),
            elePos: number = $ele.offset().top;

        $('.dropbox').append('<div class="' + className + '"></div>');

        $('.dropbox > .' + className).css({
            "position": "absolute",
            "top": elePos - dropboxPosY + "px",
            "height": eleHeight + "px",
            "width": "180px",
        });

        labelSourceParser.eachByParagraph(getParagraphId(className), (emotion: string, count: number, itr: number) =>
        {
            $('.dropbox > .' + className).append(
                '<div class="' + emotion + '" style="background-image:url(\'Content/imgs/Home/' + emotion + '-d.svg\');background-size:130px 43px;height:43px;width:130px;"><span>' +
                    count +
                '</span></div>'
            );
        });

        labelBoxController.labelPosition($('.dropbox > .' + className + ' > *').length, className);

    });

    // 貼り付けモードへ
    $('.postit-list [class]').click((event) =>
    {
        pasteMode = true;

        $('.fade-layer').css({
            "visibility": "visible",
            "opacity": 1
        });

        labelType = ((Object)(event.currentTarget)).className;
        src = 'Content/imgs/Home/' + labelType + '-d.svg';

        $('.dropbox').css({
            "opacity": 0.7,
            "z-index": 1100
        });


        $('.fade-layer, .dropbox').mousemove((e) =>
        {
            if (dropboxPosY <= e.pageY && e.pageY <= dropboxPosY + dropboxHeight)
            {
                posY = e.pageY;
            }

            if (pasteMode)
            {
                $(".dropbox > .postit-pasting").css({
                    "position": "absolute",
                    "top": posY - dropboxPosY - 20 + "px",
                    "left": "20px",
                    "z-index": 1100,
                    "visibility": "visible",
                    "background-image": "url('" + src + "')",
                    "background-size": "130px 43px"
                });
            }

            var bg;
            $('.dropbox > [class*="p-"]').each((i) => {
                var $target: JQuery = $('.dropbox > [class*="p-"]:nth-child(' + (i + 2) + ')'); // i == 0のとき１つ目のふせんを表している
                var pHeight: number = $target.outerHeight(true);
                var pPos: number = ($target.offset() || { "top": NaN }).top;
                bg = "none";
                if (pPos <= posY && posY < pPos + pHeight && pasteMode)
                {
                    bg = dropboxPosX <= e.pageX && e.pageX <= dropboxPosX + 180 ? "#24ade2" : "#c2c2bc";
                }
                
                $target.css({
                    "background": bg
                });

            });
        });

        console.log("called", labelType);
    });


    // 貼り付けて戻る
    $('.dropbox').click(() =>
    {
        $('.fade-layer').css("opacity", 0);
        setTimeout(() => {
            $('.fade-layer').css("visibility", "hidden");
        }, 500);

        $('.dropbox').css("opacity", 1);
        setTimeout(() =>
        {
            $('.dropbox').css("z-index", 0);
        }, 500);



        if (pasteMode)
        {
            pasteMode = false;

        $('.dropbox > .postit-pasting').css({
            "z-index": -100,
            "visibility": "hidden"
        });

            $('.dropbox > *').each((i) =>
            {
                var $target: JQuery = $('.dropbox > [class*="p-"]:nth-child(' + (i + 2) + ')'); // 注意
                if (!$target[0]) return true;
                var elementName: string = $target[0].tagName;
                if (elementName == "hr") return true;

                var pPos: number = ($target.offset() || { "top": NaN }).top;
                var pHeight: number = $target.outerHeight(true);

                var thisClass: string = $target.attr("class");

                var postitExistence: number =
                    $('.dropbox > [class*="p-"]:nth-child(' + (i + 1) + ') > .' + labelType).length;

                if (pPos <= posY && posY <= pPos + pHeight) // 対象のｐ要素で貼り付けた時の処理
                {
                    ajaxManager.sendPostitNumber(articleId, thisClass, labelType, postitExistence, $target, src);
                }

                // console.log($target.attr("class"), pHeight, pHeights, posY, src);
            });

        }
    });


    // 貼り付けないで戻る

    $('.fade-layer').click(() =>
    {
        $('.fade-layer').css("opacity", 0);
        setTimeout(() => {
            $('.fade-layer').css("visibility", "hidden");
        }, 500);

        $('.dropbox').css("opacity", 1);
        setTimeout(() => {
            $('.dropbox').css("z-index", 0);
        }, 500);

        $('.dropbox > .postit-pasting').css({
            "z-index": -100,
            "visibility": "hidden"
        });

        pasteMode = false;
    });



    // ふせんを貼る部分をhoverした時の処理
    $('.postit-list [class]').hover((event) =>
    {
        var thisClass: string = ((Object)(event.currentTarget)).className;
        $('.postit-list div.' + thisClass).css("visibility", "visible").animate({opacity:1},500);
    }, (event) =>
    {
        var thisClass: string = ((Object)(event.currentTarget)).className;
        $('.postit-list div.' + thisClass).css("visibility", "hidden");
    });
});