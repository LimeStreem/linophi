function updateSize()
{
    //class edit-text�̍������擾���āA�ϐ��ɑ��
    var txtH = $('.edit-text').height();
    //class edit-preview�̍������擾���擾���āA�ϐ��ɑ��
    var prevH = $('div.edit-preview').height();
    // �{�b�N�X�v�f����ʃT�C�Y�����Ȃ��łȂ���Ύ��s
    if (txtH != prevH) {
        $('div.edit-preview').height(txtH);
    }
}

var isFocus = false;
$(function ()
{
    $('.edit-text').mouseover(function ()
    {
        isFocus = true;
    });
    $(document).mouseup(function ()
    {
        isFocus = false;
    });
    $(document).mousemove(function ()
    {
        if(isFocus)updateSize();
    });

    
    updateSize();
});

$(function next_text(idx)
{
    if (window.event.keyCode == 13)
    { // 13��0x0d(CR�L�[)
        // ���̃e�L�X�g�{�b�N�X�֔�΂������������ɂ���
        document.mainFormedit[idx].focus();
        return false;
    }
    return true;
});


