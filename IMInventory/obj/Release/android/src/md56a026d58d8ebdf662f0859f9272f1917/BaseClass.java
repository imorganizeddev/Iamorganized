package md56a026d58d8ebdf662f0859f9272f1917;


public class BaseClass
	extends md56a026d58d8ebdf662f0859f9272f1917.CommonFunction
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("IMInventory.BaseClass, IMInventory", BaseClass.class, __md_methods);
	}


	public BaseClass ()
	{
		super ();
		if (getClass () == BaseClass.class)
			mono.android.TypeManager.Activate ("IMInventory.BaseClass, IMInventory", "", this, new java.lang.Object[] {  });
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
