package md5a9724588c3e09fba5144f226d6d6e3dd;


public class StripeView
	extends android.widget.LinearLayout
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("Stripe.StripeView, Stripe", StripeView.class, __md_methods);
	}


	public StripeView (android.content.Context p0)
	{
		super (p0);
		if (getClass () == StripeView.class)
			mono.android.TypeManager.Activate ("Stripe.StripeView, Stripe", "Android.Content.Context, Mono.Android", this, new java.lang.Object[] { p0 });
	}


	public StripeView (android.content.Context p0, android.util.AttributeSet p1)
	{
		super (p0, p1);
		if (getClass () == StripeView.class)
			mono.android.TypeManager.Activate ("Stripe.StripeView, Stripe", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android", this, new java.lang.Object[] { p0, p1 });
	}


	public StripeView (android.content.Context p0, android.util.AttributeSet p1, int p2)
	{
		super (p0, p1, p2);
		if (getClass () == StripeView.class)
			mono.android.TypeManager.Activate ("Stripe.StripeView, Stripe", "Android.Content.Context, Mono.Android:Android.Util.IAttributeSet, Mono.Android:System.Int32, mscorlib", this, new java.lang.Object[] { p0, p1, p2 });
	}

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
