using CommunityToolkit.Mvvm.Messaging;
namespace DPkarta
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
        protected override void OnResume()
        {
            WeakReferenceMessenger.Default.Send(new Message() { message = "resume" });
            base.OnResume();
        }
        protected override void OnSleep()
        {
            WeakReferenceMessenger.Default.Send(new Message() { message = "sleep" });
            base.OnSleep();
        }
    }
    public class Message()
    {
        public required string message { get; set; }
    }
}
