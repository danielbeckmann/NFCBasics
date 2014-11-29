using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Proximity;
using Windows.Phone.Devices.Notification;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace NfcBasics
{
    /// <summary>
    /// Page for writing uri messages to nfc tags.
    /// </summary>
    public sealed partial class WriteTagPage : Page
    {
        /// <summary>
        /// The nfc communication device.
        /// </summary>
        private ProximityDevice device;

        /// <summary>
        /// Id for the subscription, this is used to unsubscribe, when the subscription is no longer needed.
        /// </summary>
        private long publishedMessageId;

        public WriteTagPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Handle hardware back button
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            // Get the nfc device
            this.device = ProximityDevice.GetDefault();

            // Start publishing the default message
            this.StartPublish();
        }

        /// <summary>
        /// When the text in the textbox changes, restart publishing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WriteTagContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (device != null)
            {
                // Stops the publishing of the current message
                this.device.StopPublishingMessage(this.publishedMessageId);
            }

            // Starts publishing the new message
            this.StartPublish();
        }

        private async void StartPublish()
        {
            // Test if nfc is available
            if (device != null)
            {
                // Check the uri format
                if (Uri.IsWellFormedUriString(this.WriteTagContent.Text, UriKind.RelativeOrAbsolute))
                {
                    // Get the uri to publish
                    var publishUri = new Uri(WriteTagContent.Text, UriKind.RelativeOrAbsolute);

                    // Convert the data to the needed Buffer
                    var bytes = Encoding.Unicode.GetBytes(publishUri.ToString());
                    var buffer = bytes.AsBuffer();

                    // Start publishing the message, store the message id for later unsubscription
                    this.publishedMessageId = this.device.PublishBinaryMessage("WindowsUri:WriteTag", buffer, PublishedHandler);
                }
                else
                {
                    await new MessageDialog("No valid URI entered").ShowAsync();
                }
            }
            else
            {
                await new MessageDialog("Your phone has no NFC or it is disabled").ShowAsync();
            }
        }

        /// <summary>
        /// Invoked when nfc message was published.
        /// </summary>
        /// <param name="sender">The nfc device the message was subscribed with</param>
        /// <param name="messageId">The id of the written message</param>
        private void PublishedHandler(ProximityDevice sender, long messageId)
        {
            // Let the device vibrate to indicate that a new message was written
            var vibrationDevice = VibrationDevice.GetDefault();
            if (vibrationDevice != null)
            {
                vibrationDevice.Vibrate(TimeSpan.FromSeconds(0.2));
            }

            // Stops the publishing of the current message
            sender.StopPublishingMessage(messageId);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            Windows.Phone.UI.Input.HardwareButtons.BackPressed -= HardwareButtons_BackPressed;

            if (device != null)
            {
                // Stops the publishing of the current message
                this.device.StopPublishingMessage(this.publishedMessageId);
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

        private void WriteWifi_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.WriteTagContent.Text = "ms-settings-wifi:";
        }

        private void WriteBluetooth_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.WriteTagContent.Text = "ms-settings-bluetooth:";
        }

        private void WriteMail_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.WriteTagContent.Text = "mailto:daniel@panoshooters.de?subject=NFC%20ist%20praktisch!";
        }
    }
}
