namespace Web.Utility.OGP
{
    public interface IOGPSiteMeta
    {
        /// <summary>
        /// �T�C�g��
        /// </summary>
        string SiteName { get; }
        
        /// <summary>
        /// �C���[�W���w�肳��ĂȂ��Ƃ��ɗ��p����f�t�H���g�̃C���[�W
        /// </summary>
        string DefaultImage { get; }

        /// <summary>
        /// �f�t�H���g�̃��P�[��
        /// </summary>
        string DefaultLocale { get; }
    }
}