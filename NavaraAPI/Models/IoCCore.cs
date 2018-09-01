using Ninject;
using NavaraAPI;
using SmartLifeLtd.ViewModels;

namespace NavaraAPI.Models
{
    /// <summary>
    /// The IoC class to work with golobal classes and with the ui
    /// </summary>
    public static class IoCCore
    {
        #region SingleTone instance
        /// <summary>
        /// The kernal to bind values to 
        /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();
        #endregion

        #region Shortcuts
        /// <summary>
        /// A shortcut to access <see cref="ApplicationViewModel"/>
        /// </summary>
        public static ApplicationViewModel AppViewModel { get; private set; } = Kernel.Get<ApplicationViewModel>();
        #endregion

        #region Constructer
        /// <summary>
        /// Is called on the start of the application to construct the kernal
        /// </summary>
        public static void SetUp()
        {
            BindViewModels();
        }
        /// <summary>
        /// Binds the viewModels to the values
        /// </summary>
        private static void BindViewModels()
        {
            Kernel.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Gets the view molde 
        /// </summary>
        /// <typeparam name="T">The type of the viewmodel that we want to get</typeparam>
        public static T Get<T>()
        {
            return Kernel.Get<T>();
        }
        #endregion
    }
}
