package md56a026d58d8ebdf662f0859f9272f1917;


public class GoogleDriveWebview_MyWebViewClient
	extends android.webkit.WebViewClient
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onPageStarted:(Landroid/webkit/WebView;Ljava/lang/String;Landroid/graphics/Bitmap;)V:GetOnPageStarted_Landroid_webkit_WebView_Ljava_lang_String_Landroid_graphics_Bitmap_Handler\n" +
			"n_onPageFinished:(Landroid/webkit/WebView;Ljava/lang/String;)V:GetOnPageFinished_Landroid_webkit_WebView_Ljava_lang_String_Handler\n" +
			"";
		mono.android.Runtime.register ("IMInventory.GoogleDriveWebview+MyWebViewClient, IMInventory", GoogleDriveWebview_MyWebViewClient.class, __md_methods);
	}


	public GoogleDriveWebview_MyWebViewClient ()
	{
		super ();
		if (getClass () == GoogleDriveWebview_MyWebViewClient.class)
			mono.android.TypeManager.Activate ("IMInventory.GoogleDriveWebview+MyWebViewClient, IMInventory", "", this, new java.lang.Object[] {  });
	}

	public GoogleDriveWebview_MyWebViewClient (android.webkit.WebView p0, android.app.Activity p1)
	{
		super ();
		if (getClass () == GoogleDriveWebview_MyWebViewClient.class)
			mono.android.TypeManager.Activate ("IMInventory.GoogleDriveWebview+MyWebViewClient, IMInventory", "Android.Webkit.WebView, Mono.Android:Android.App.Activity, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public void onPageStarted (android.webkit.WebView p0, java.lang.String p1, android.graphics.Bitmap p2)
	{
		n_onPageStarted (p0, p1, p2);
	}

	private native void n_onPageStarted (android.webkit.WebView p0, java.lang.String p1, android.graphics.Bitmap p2);


	public void onPageFinished (android.webkit.WebView p0, java.lang.String p1)
	{
		n_onPageFinished (p0, p1);
	}

	private native void n_onPageFinished (android.webkit.WebView p0, java.lang.String p1);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
