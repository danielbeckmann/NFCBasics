using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Networking.Proximity;
using Windows.Phone.Devices.Notification;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace NfcBasics
{
    /// <summary>
    /// Page for reading uri messages from nfc tags.
    /// </summary>
    public sealed partial class ReadTagPage : Page
    {
        /// <summary>
        /// The nfc communication device.
        /// </summary>
        private ProximityDevice device;

        /// <summary>
        /// Id for the subscription, this is used to unsubscribe, when the subscription is no longer needed.
        /// </summary>
        private long subscribedMessageId;

        public ReadTagPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Handle hardware back button
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            // Get the nfc device
            this.device = ProximityDevice.GetDefault();

            // Test if nfc is available
            if (device != null)
            {
                // Subscribe to uri messages
                this.subscribedMessageId = device.SubscribeForMessage("WindowsUri", this.MessageReceivedHandler);
            }
            else
            {
                await new MessageDialog("Your phone has no NFC or it is disabled").ShowAsync();
            }
        }

        /// <summary>
        /// Handles the incoming messages.
        /// </summary>
        /// <param name="device">The nfc device, the message is read from</param>
        /// <param name="message">The incoming message</param>
        private async void MessageReceivedHandler(ProximityDevice device, ProximityMessage message)
        {
            // Get the data
            var buffer = message.Data.ToArray();

            // Decode the message
            var uri = Encoding.Unicode.GetString(buffer, 0, buffer.Length);

            // Display result in text area - use the dispatcher, as the reading occurs in a separate thread
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.TextArea.Text = uri;
            });

            // Let the device vibrate to indicate that a new message was read
            var vibrationDevice = VibrationDevice.GetDefault();
            if (vibrationDevice != null)
            {
                vibrationDevice.Vibrate(TimeSpan.FromSeconds(0.2));
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;

            if (device != null)
            {
                // Unsubscribe the handler
                this.device.StopSubscribingForMessage(this.subscribedMessageId);
            }
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                e.Handled = true;
            }
        }
    }
}
