using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Util;
using Android.Widget;
using Xamarin.Essentials;
using static Android.Telecom.Call;

namespace HelloAppWidget
{
    [BroadcastReceiver(Label = "HellApp Widget")]
	[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
	[MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
	public class AppWidget : AppWidgetProvider
	{
		private static readonly string AnnouncementClick = "AnnouncementClickTag";

		private IDataFromApi _api;
        private IDataFromApi Api
        {
            get
            {
				if(_api == null)
                {
					_api = new DataApi();
                }
				return _api;
            }
        }


        public override async void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
		{
			var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);

            var vattenfallSpotPrices = await Api.GetDataFromVattenFallAsync().ConfigureAwait(false);

			if(vattenfallSpotPrices == null)
            {
				return;
            }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds, vattenfallSpotPrices));
            });
		}
        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Check if the click is from the "Announcement" button
            if (AnnouncementClick.Equals(intent.Action))
            {
                var pm = context.PackageManager;
                try
                {
                    var packageName = "com.android.settings";
                    var launchIntent = pm.GetLaunchIntentForPackage(packageName);
                    context.StartActivity(launchIntent);
                }
                catch
                {
                    // Something went wrong :)
                }
            }
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds, IEnumerable<VattenfallSpotPrice> vattenfallSpotPrices)
		{
			// Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
			var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);

			SetTextViewText(widgetView, vattenfallSpotPrices);
			RegisterClicks(context, appWidgetIds, widgetView);

			return widgetView;
		}

		private void SetTextViewText(RemoteViews widgetView, IEnumerable<VattenfallSpotPrice> vattenfallSpotPrices)
		{
            var only3rowOfData = vattenfallSpotPrices.Take(3).ToArray();



            for(int i = 0; i< 3; i++)
            {
                string smallText = $"Last at: {only3rowOfData[i].TimeStampHour}";
                string mediumText = $"Price {only3rowOfData[i].Value} {only3rowOfData[i].Unit}";

                if (i == 0)
                {
                    widgetView.SetTextViewText(Resource.Id.widgetSmall, smallText);
                    widgetView.SetTextViewText(Resource.Id.widgetMedium, mediumText);
                }
                else if(i == 1)
                {
                    widgetView.SetTextViewText(Resource.Id.widgetSmall2, smallText);
                    widgetView.SetTextViewText(Resource.Id.widgetMedium2, mediumText);
                }
                else if (i == 2)
                {
                    widgetView.SetTextViewText(Resource.Id.widgetSmall3, smallText);
                    widgetView.SetTextViewText(Resource.Id.widgetMedium3, mediumText);
                }

            }


        }

        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
		{
			var intent = new Intent(context, typeof(AppWidget));
			intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
			intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

			// Register click event for the Background
			var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
			widgetView.SetOnClickPendingIntent(Resource.Id.widgetBackground, piBackground);


		}

		private PendingIntent GetPendingSelfIntent(Context context, string action)
		{
			var intent = new Intent(context, typeof(AppWidget));
			intent.SetAction(action);
			return PendingIntent.GetBroadcast(context, 0, intent, 0);
		}

	}
}
