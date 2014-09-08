using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Ajax.Utilities;
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

        [HttpGet]
        [AllowAnonymous]
        public ActionResult LogIn(string returnUrl)
        {
            if (User.Identity.IsAuthenticated) return Redirect("/");
            returnUrl = returnUrl ?? "/";
            return View((object)returnUrl);
        }
        [Authorize]
        [HttpGet]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();
            return View();
        }

        
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> Config()
        {
            UserAccount user =await UserManager.FindByNameAsync(User.Identity.Name);
            return View(new AccountConfigViewModel()
            {
                MailMagazine = user.AcceptEmail,
                NickName = user.NickName,
            });
        }

        [ValidateAntiForgeryToken]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Config(AccountConfigViewModel vm)
        {
            UserAccount user = await UserManager.FindByNameAsync(User.Identity.Name);
            user.NickName = vm.NickName;
            user.AcceptEmail = vm.MailMagazine;
            user.UpdateTime=DateTime.Now;
            await UserManager.UpdateAsync(user);
            return RedirectToAction("Config");
        }

        [AllowAnonymous]//TODO:���J���̍폜
        public ActionResult TestConfirmation()
        {
#if DEBUG
            return View("ExternalLoginConfirmation");
#else
            return null;
#endif
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

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel vm)
        {
            if (vm == null) return View("Page403");
            if (User.Identity.IsAuthenticated||!vm.AcceptTerm) return RedirectToAction("LogIn");
            if (ModelState.IsValid)
            {
                ExternalLoginInfo info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = UserAccount.CreateUser(info.Login.LoginProvider + info.Login.ProviderKey, vm.NickName,
                    vm.Email,vm.AcceptMail);
                IdentityResult result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInAsync(user, false);

                        // �A�J�E���g�m�F�ƃp�X���[�h ���Z�b�g��L���ɂ�����@�̏ڍׂɂ��ẮAhttp://go.microsoft.com/fwlink/?LinkID=320771 ���Q�Ƃ��Ă�������
                        // ���̃����N���܂ޓd�q���[���𑗐M���܂�
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // SendEmail(user.Email, callbackUrl, "�A�J�E���g�̊m�F", "���̃����N���N���b�N���邱�Ƃɂ���ăA�J�E���g���m�F���Ă�������");

                        return RedirectToLocal(vm.ReturnUrl);
                    }
                }
            }
            return View(vm);
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

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }

    public class ExternalLoginConfirmationViewModel
    {
        public string ReturnUrl { get; set; }
        [Display(Name = "�j�b�N�l�[��")]
        public string NickName { get; set; }
        [Display(Name = "���[���A�h���X")]
        public string Email { get; set; }
        [Display(Name = "���p�K��ɓ��ӂ���")]
        public bool AcceptTerm { get; set; }
        [Display(Name = "linophi�̒ʒm�����[���Ŏ󂯎��")]
        public bool AcceptMail { get; set; }
    }


}