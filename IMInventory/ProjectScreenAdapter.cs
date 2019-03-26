using System;
using Android.Widget;
using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Runtime;
using Android.Graphics;

namespace IMInventory
{
	public class ProjectScreenAdapter : BaseAdapter<TableItem>
	{
		List<TableItem> items;
   		Activity context;

        //private ItemFilter mFilter = new ItemFilter();
        
		public ProjectScreenAdapter (Activity context, List<TableItem> items) : base()
		{
			this.context = context;
       		this.items = items;
		}
		public override long GetItemId(int position)
		{
			return position;
		}
		public override TableItem this[int position]
		{
			get { return items[position]; }
		}
		public override int Count
		{
			get { return items.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var item = items[position];
			View view = convertView;
			if (view == null) // no view to re-use, create new
			   view = context.LayoutInflater.Inflate(Resource.Layout.SearchProject, null);
			Typeface tf=Typeface.CreateFromAsset(context.Assets,"Fonts/ROBOTO-LIGHT.TTF");
			view.FindViewById<TextView>(Resource.Id.TextProject1).Text = item.Projectname;
			view.FindViewById<TextView>(Resource.Id.TextProject1).Typeface=tf;
			view.FindViewById<TextView>(Resource.Id.TextProject1).Invalidate();

			view.FindViewById<TextView>(Resource.Id.TextProject2).Text =item.addeddate;
			view.FindViewById<TextView>(Resource.Id.TextProject2).Typeface=tf;
			view.FindViewById<TextView>(Resource.Id.TextProject2).Invalidate();

			view.FindViewById<TextView>(Resource.Id.textId).Text = item.ProjectID;
			view.FindViewById<TextView>(Resource.Id.textId).Typeface=tf;
			view.FindViewById<TextView>(Resource.Id.textId).Invalidate();

			view.FindViewById<TextView>(Resource.Id.TextClientName).Text = item.ClientName;
			view.FindViewById<TextView>(Resource.Id.TextClientName).Typeface=tf;
			view.FindViewById<TextView>(Resource.Id.TextClientName).Invalidate();

			return view;
		}

        //public Filter getFilter()
        //{
        //    return mFilter;
        //}
	}

    //public class ItemFilter : Filter 
    //{
    //   private List<String>originalData = null;
    //    private List<String>filteredData = null;

    //    protected FilterResults performFiltering(CharSequence constraint)
    //    {

    //        String filterString = constraint.ToString().ToLower();
    //        FilterResults results = new FilterResults();

    //        List<String> list = originalData;

    //        int count = list.Count;
    //        String[] nlist = new String[count];
    //        int nlistindex = 0;
    //        String filterableString ;

    //        for (int i = 0; i < count; i++) {
    //            filterableString = list[i];
    //            if (filterableString.ToLower().Contains(filterString)) {
    //                nlist.SetValue(filterableString,nlistindex);
    //                nlistindex++;
    //            }
    //        }

    //        results.Values = nlist;
    //        results.Count = nlist.Length;

    //        return results;
    //    }

    //    protected void publishResults(CharSequence constraint, FilterResults results) {
    //        filteredData = new List<string>(results.Values.ToArray<String>());
    //        //notifyDataSetChanged();
    //    }

    //}
}


