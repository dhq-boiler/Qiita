using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Question20220124.Extensions
{
    public static class Extensions
    {
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
    }
}
