using System.Collections.Generic;
using AndroidX.Fragment.App;
using Java.Lang;
using QuickDate.Helpers.Utils;
using Exception = Java.Lang.Exception;
using SupportFragment = AndroidX.Fragment.App.Fragment;
 
namespace QuickDate.Adapters
{
    public class MainTabAdapter : FragmentStatePagerAdapter
    {
        private List<SupportFragment> Fragments { get; set; }
        private List<string> FragmentNames { get; set; }

#pragma warning disable 618
        public MainTabAdapter(FragmentManager fm) : base(fm)
#pragma warning restore 618
        {
            try
            {
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public MainTabAdapter(FragmentManager fm, int behavior) : base(fm, behavior)
        {
            try
            {
                Fragments = new List<SupportFragment>();
                FragmentNames = new List<string>();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        public void AddFragment(SupportFragment fragment, string name)
        {
            try
            {
                Fragments.Add(fragment);
                FragmentNames.Add(name);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ClearFragment()
        {
            try
            {
                Fragments.Clear();
                FragmentNames.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveFragment(SupportFragment fragment, string name)
        {
            try
            {
                Fragments.Remove(fragment);
                FragmentNames.Remove(name);
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertFragment(int index, SupportFragment fragment, string name)
        {
            try
            {
                Fragments.Insert(index, fragment);
                FragmentNames.Insert(index, name);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override int Count
        {
            get { return Fragments.Count; }
        }

        public override SupportFragment GetItem(int position)
        {
            try
            {
                if (Fragments[position] != null)
                {
                    return Fragments[position];
                }
                else
                {
                    return Fragments[0];
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            return new Java.Lang.String(FragmentNames[position]);
        }

         
       
    }
}