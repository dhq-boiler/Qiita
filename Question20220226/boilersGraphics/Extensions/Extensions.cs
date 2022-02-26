using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Extensions
{
    public static class Extensions
    {

        /*
         * https://stackoverflow.com/questions/10279092/how-to-get-children-of-a-wpf-container-by-type
         */
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static IEnumerable<T> EnumerateChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as IEnumerable<T>) ?? EnumerateChildOfType<T>(child);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        if (item != null)
                            yield return item;
                    }
                }
                var result2 = (child as T) ?? GetChildOfType<T>(child);
                if (result2 != null)
                    yield return result2;
            }
        }


        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                if (depObj is Prism.Services.Dialogs.DialogWindow dw)
                {
                    var loadObj = dw.Template.LoadContent();
                    foreach (var childOfChild in FindVisualChildren<DependencyObject>(loadObj))
                    {
                        if (childOfChild is ContentPresenter cp)
                        {
                            var content = cp.Content as DependencyObject;
                            if (content is T)
                            {
                                yield return (T)content;
                            }
                            foreach (var childOfChild2 in FindVisualChildren<DependencyObject>(content))
                            {
                                if (childOfChild2 != null && childOfChild2 is T)
                                {
                                    yield return (T)childOfChild2;
                                }
                            }
                            if (cp.ContentTemplate != null)
                            {
                                var loadObj2 = cp.ContentTemplate.LoadContent();
                                if (loadObj2 is T)
                                {
                                    yield return (T)loadObj2;
                                }
                                foreach (var childOfChild2 in FindVisualChildren<DependencyObject>(loadObj2))
                                {
                                    if (childOfChild2 != null && childOfChild2 is T)
                                    {
                                        yield return (T)childOfChild2;
                                    }
                                }
                            }
                        }
                        if (childOfChild is Control c)
                        {
                            if (c.Template != null)
                            {
                                var loadObj2 = c.Template.LoadContent();
                                if (loadObj2 is T)
                                {
                                    yield return (T)loadObj2;
                                }
                                foreach (var childOfChild2 in FindVisualChildren<DependencyObject>(loadObj2))
                                {
                                    if (childOfChild2 != null && childOfChild2 is T)
                                    {
                                        yield return (T)childOfChild2;
                                    }
                                }
                            }
                        }
                        if (childOfChild is ContentControl cc)
                        {
                            if (cc.Template != null)
                            {
                                var loadObj2 = cc.Template.LoadContent();
                                if (loadObj2 is T)
                                {
                                    yield return (T)loadObj2;
                                }
                                foreach (var childOfChild2 in FindVisualChildren<DependencyObject>(loadObj2))
                                {
                                    if (childOfChild2 != null && childOfChild2 is T)
                                    {
                                        yield return (T)childOfChild2;
                                    }
                                }
                            }

                            if (cc.ContentTemplate != null)
                            {
                                var loadObj2 = cc.ContentTemplate.LoadContent();
                                if (loadObj2 is T)
                                {
                                    yield return (T)loadObj2;
                                }
                                foreach (var childOfChild2 in FindVisualChildren<DependencyObject>(loadObj2))
                                {
                                    if (childOfChild2 != null && childOfChild2 is T)
                                    {
                                        yield return (T)childOfChild2;
                                    }
                                }
                            }
                        }
                        if (childOfChild != null && childOfChild is T)
                        {
                            yield return (T)childOfChild;
                        }
                    }

                    if (dw.ContentTemplate != null)
                    {
                        loadObj = dw.ContentTemplate.LoadContent();
                        foreach (var childOfChild in FindVisualChildren<DependencyObject>(loadObj))
                        {
                            if (childOfChild != null && childOfChild is T)
                            {
                                yield return (T)childOfChild;
                            }
                        }
                    }
                }

                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    if (child != null && child is ContentPresenter cp && cp.ContentTemplate != null)
                    {
                        var dependencyObject = cp.ContentTemplate.LoadContent();
                        if (dependencyObject is T)
                        {
                            yield return (T)dependencyObject;
                        }
                        foreach (T childOfChild in FindVisualChildren<T>(dependencyObject))
                        {
                            yield return childOfChild;
                        }
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static IEnumerable<FrameworkElement> GetChildren(this FrameworkElement parent)
        {
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;

                var children = GetChildren(child).ToList();
                foreach (var child2 in children)
                    yield return child2;

                if (child != null)
                    yield return child;
            }
        }

        public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static IEnumerable<T> GetCorrespondingViews<T>(this FrameworkElement parent, object dataContext, bool parentInclude = false)
            where T : FrameworkElement
        {
            if (parentInclude && parent.DataContext == dataContext)
            {
                if (parent is T)
                    yield return parent as T;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                var result = (child as IEnumerable<T>) ?? EnumerateChildOfType<T>(child);
                if (result != null)
                {
                    foreach (var item in result)
                    {
                        if (item != null && item.DataContext == dataContext)
                            yield return item;
                    }
                }
                var result2 = (child as T) ?? GetChildOfType<T>(child);
                if (result2 != null && result2.DataContext == dataContext)
                    yield return result2;
            }
        }

        public static DependencyObject GetParentOfType(this DependencyObject obj, string name)
        {
            if (obj == null) return null;

            while (obj != null && (obj is FrameworkElement && !(obj as FrameworkElement).Name.Equals(name)))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (obj == null) return null;

            return obj;
        }

        public static double GetAvgWidth(this GlyphTypeface glyphTypeface, int fontSize)
        {
            const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            double ret = 0d;
            foreach (var @char in str)
            {
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
                PathGeometry pg = geometry.GetOutlinedPathGeometry();
                ret += pg.Bounds.Width;
            }
            return ret / str.Count();
        }

        public static double GetAvgHeight(this GlyphTypeface glyphTypeface, int fontSize)
        {
            const string str = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            double ret = 0d;
            foreach (var @char in str)
            {
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
                PathGeometry pg = geometry.GetOutlinedPathGeometry();
                ret += pg.Bounds.Height;
            }
            return ret / str.Count();
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return new ObservableCollection<T>(source);
        }


        public static T GetParent<T>(this DependencyObject obj)
        {
            var parent = VisualTreeHelper.GetParent(obj);
            return parent switch
            {
                null => default,
                T ret => ret,
                _ => parent.GetParent<T>()
            };
        }

        public static IEnumerable<DependencyObject> Children(this DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var count = VisualTreeHelper.GetChildrenCount(obj);
            if (count == 0)
                yield break;

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                yield return child;
            }
        }

        public static IEnumerable<DependencyObject> Descendants(this DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            foreach (var child in obj.Children())
            {
                yield return child;
                foreach (var grandChild in child.Descendants())
                    yield return grandChild;
            }
        }

        //--- 特定の型の子孫要素を取得
        public static IEnumerable<T> Descendants<T>(this DependencyObject obj)
            where T : DependencyObject
        {
            return obj.Descendants().OfType<T>();
        }

        //https://stackoverflow.com/questions/41608665/linq-recursive-parent-child
        public static IEnumerable<T2> SelectRecursive<T1, T2>(this IEnumerable<T1> source, Func<T2, IEnumerable<T2>> selector) where T1 : class where T2 : class
        {
            var ret = new ConcurrentBag<T2>();
            Parallel.For(0, source.Count(), i =>
            {
                var parent = source.ElementAt(i);
                ret.Add(parent as T2);

                var children = selector(parent as T2);
                var c = SelectRecursive(children, selector);
                for (int j = 0; j < c.Count(); j++)
                {
                    var child = c.ElementAt(j);
                    ret.Add(child);
                }
            });
            return ret;
        }

        public static bool HasAsAncestor(this LayerTreeViewItemBase layerItem, LayerTreeViewItemBase ancestor)
        {
            LayerTreeViewItemBase temp = layerItem;
            while (temp.Parent.Value != null)
            {
                if (temp == ancestor)
                    return true;
                temp = temp.Parent.Value;
            }
            return false;
        }

        //http://stackoverflow.com/questions/7319952/how-to-get-listbox-itemspanel-in-code-behind
        public static T GetVisualChild<T>(this DependencyObject parent) where T : Visual
        {
            T child = default(T);

            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
