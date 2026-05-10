using UnityEngine;

public class DevicePlugin
{
	public static AndroidJavaClass androidplatform;

	public static void InitAndroidPlatform()
	{
		//if (Application.platform == RuntimePlatform.Android)
		//{
		//	androidplatform = new AndroidJavaClass("com.trinitigame.androidplatform.AndroidPlatformActivity");
		//}
	}

	public static void AndroidQuit()
	{
		//androidplatform.CallStatic("AndroidQuit");
	}

	public static string GetDeviceModel()
	{
		return string.Empty;
	}

	public static string GetDeviceModelDetail()
	{
		return "";
		//return androidplatform.CallStatic<string>("GetAndroidVersion", new object[0]);
	}

	public static string GetUUID()
	{
		return "";
		//return androidplatform.CallStatic<string>("GetUUID", new object[0]);
	}

	public static string GetCountryCode()
	{
		return "";
		//return androidplatform.CallStatic<string>("GetCountry", new object[0]);
	}

	public static string GetLanguageCode()
	{
		return "";
		//return Application.systemLanguage.ToString();
	}

	public static string GetSysVersion()
	{
		return "";
		//return androidplatform.CallStatic<string>("GetAndroidVersion", new object[0]);
	}

	public static string GetAppVersion()
	{
		return "";
		//return androidplatform.CallStatic<string>("GetAndroidAPPVersion", new object[0]);
	}

	public static string GetAppBundleId()
	{
		return "";
		//return androidplatform.CallStatic<string>("GetPackgeName", new object[0]);
	}
}
