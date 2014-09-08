﻿var CommentSourceParser = (function () {
    function CommentSourceParser() {
        this.commentJson = JSON.parse($("#comment-info").text());
    }
    CommentSourceParser.prototype.eachDoInComments = function (paragraphId, handler) {
        for (var i = 0, len = this.commentJson.length; i < len; i++) {
            if (this.commentJson[i]["ParagraphId"] == paragraphId) {
                handler(this.commentJson[i]["Name"], this.commentJson[i]["PostTime"], this.commentJson[i]["AutoId"], this.commentJson[i]["Comment"]);
            }
        }
    };
    return CommentSourceParser;
})();

$(function () {
    var commentSourceParser = new CommentSourceParser();

    // コメントの表示に関する
    $('.article-container > *').each(function (i) {
        var $ele = $('[class^="x_p-"]:nth-child(' + (i + 1) + ')');
        var className = $ele.attr("class");

        $('.widget .' + className).append('<div class="' + className + '-comments"></div>');

        commentSourceParser.eachDoInComments(className.substr(4), function (name, time, id, comment) {
            $('.widget .' + className + '-comments').append('<div class="response">' + '<p class="res-title"> counter <b>' + name + '</b> <small>[' + time + '] ID:' + id + ' </small> </p>' + '<p class="res-text">' + comment + '</p>' + '</div>');
        });
        $('.widget .' + className + '-comments').append('<button class="' + className + '">コメントを残す</button>');
    });

    $('.widget button').on("click", function () {
        var formHtml = '<form id="the-form" action="/echo/json" method="POST">' + '<input type="text" name="name" value="" placeholder="Name" />' + '<textarea name="message" placeholder="Messages"></textarea>' + '<button>送信！</button>' + '</form>';

        $().alertwindow(formHtml, "none", "コメントを残す", function () {
        });
    });
});
//# sourceMappingURL=CommentIndicator.js.map
