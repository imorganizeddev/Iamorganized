package md56a026d58d8ebdf662f0859f9272f1917;


public class CommonFunction
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("IMInventory.CommonFunction, IMInventory", CommonFunction.class, __md_methods);
	}


	public CommonFunction ()
	{
		super ();
		if (getClass () == CommonFunction.class)
			mono.android.TypeManager.Activate ("IMInventory.CommonFunction, IMInventory", "", this, new java.lang.Object[] {  });
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
