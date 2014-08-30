using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Web.Models;
using Web.Storage;
using Web.Utility;

namespace Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }

        [AllowAnonymous]
        public ActionResult LogIn()
        {
            return View();
        }

        public ActionResult SignUp()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult TestConfirmation()
        {
            return View("ExternalLoginConfirmation");
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ExternalLogin(string provider,string returnUrl)
        {
            return new ChallengeResult(provider,Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {

            ExternalLoginInfo loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            //Owin Security�ɂ���Ē񋟂���郍�O�C����Ԃ̎擾�B
            if (loginInfo == null) //���O�C���ł��Ȃ������ꍇ?(�F�؃g�[�N���̎擾���s)
            {
                return RedirectToAction("Login"); //Login�̃y�[�W�ɖ߂��B
            }

            // ���[�U�[�����Ƀ��O�C���������Ă���ꍇ�A���̊O�����O�C�� �v���o�C�_�[���g�p���ă��[�U�[���T�C���C�����܂�
            UserAccount user = await UserManager.FindAsync(loginInfo.Login);
            if (user != null)
            {
                await SignInAsync(user, false);
                return Redirect(returnUrl);
            }
            // ���[�U�[���A�J�E���g�������Ă��Ȃ��ꍇ�A���[�U�[�ɃA�J�E���g���쐬����悤���߂܂�
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
            //��)���̏u�ԂɈȉ��̃R�[�h��ExternalLoginConfirmation�A�N�V�������Ăяo���Ă���̂ł͂Ȃ��B
            //�����܂ł��r���[�����^�[�����Ă���ɉ߂��Ȃ����Ƃɒ��ӁB
            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email }
                /*�t�H�[���̃f�t�H���g�l�Ƃ��ă��[���A�h���X���Z�b�g���邽�߁B*/);
        }

        private async Task SignInAsync(UserAccount user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent },
                await user.GenerateUserIdentityAsync(UserManager));
        }

        private IAuthenticationManager AuthenticationManager
        {
            get { return HttpContext.GetOwinContext().Authentication; }
        }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Display(Name = "���[���A�h���X")]
        public string Email { get; set; }
        [Display(Name = "���p�K��ɓ��ӂ���")]
        public bool AcceptTerm { get; set; }
        [Display(Name = "linophi�̒ʒm�����[���Ŏ󂯎��")]
        public bool AcceptMail { get; set; }
    }
}