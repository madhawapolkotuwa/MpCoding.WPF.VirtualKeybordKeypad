using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace VirtualKeys.Controls
{
    public static class Helpers
    {
        public enum GraphSearch
        {
            DepthFirst,
            BreadthFirst
        }

        /// <summary>
        ///     Returns a list of child elements which are of type T.
        /// </summary>
        /// <typeparam name="T">Requested Type</typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    var child = VisualTreeHelper.GetChild(depObj, i);
                    var children = child as T;
                    if (children != null)
                    {
                        yield return children;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        ///     Returns the first child element from the VisualTree that is of type T. The algorithm works depth-first.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="depObj"></param>
        /// <returns></returns>
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null)
            {
                return null;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = child as T ?? GetChildOfType<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     Finds a parent element from the Visual Tree that matches the given Predicate
        /// </summary>
        /// <param name="child"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        public static DependencyObject FindParent(this DependencyObject child, Predicate<DependencyObject> search)
        {
            //get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }

            if (search(parentObject))
            {
                return parentObject;
            }

            return FindParent(parentObject, search);
        }

        /// <summary>
        ///     Finds a parent element from the Visual Tree that matches the given Predicate. Only iterates the amount of maxDepth
        /// </summary>
        /// <param name="child"></param>
        /// <param name="search"></param>
        /// <param name="maxDepth">How many levels are iterated</param>
        /// <returns></returns>
        public static DependencyObject FindParent(this DependencyObject child, Predicate<DependencyObject> search,
            int maxDepth)
        {
            if (maxDepth <= 0)
            {
                return null;
            }

            //get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }


            if (search(parentObject))
            {
                return parentObject;
            }

            return FindParent(parentObject, search, --maxDepth);
        }

        /// <summary>
        ///     Finds the first parent of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            var parentObject = VisualTreeHelper.GetParent(child);

            //we've reached the end of the tree
            if (parentObject == null)
            {
                return null;
            }

            //check if the parent matches the type we're looking for
            var parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }

            return FindParent<T>(parentObject);
        }

        /// <summary>
        ///     Finds a Child of a given item with given name in the visual tree.
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns></returns>
        public static T FindChild<T>(DependencyObject parent, string childName)
            where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
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

        /// <summary>
        ///     Finds a resource of type T from given resourceDictionary
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName"></param>
        /// <param name="resourceDictionaryName"></param>
        /// <returns></returns>
        public static T FindResource<T>(string resourceName, string resourceDictionaryName)
        {
            try
            {
                var resourceDictionary = new ResourceDictionary
                {
                    Source = new Uri(resourceDictionaryName, UriKind.Relative)
                };

                return (T)resourceDictionary[resourceName];
            }
            catch (ResourceReferenceKeyNotFoundException)
            {
                //stuff here to hande
                return default;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        ///     Checks whether the element is a child element of another element
        /// </summary>
        /// <param name="depObj"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static bool IsChildOf(this DependencyObject depObj, DependencyObject parent)
        {
            if (depObj == null || parent == null)
            {
                return false;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (ReferenceEquals(child, depObj) || depObj.IsChildOf(child))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Finds a Child element that meets the conditions given by the predicate
        /// </summary>
        /// <param name="root"></param>
        /// <param name="search">condition that needs to match</param>
        /// <param name="graphSearchType">Traverse using Breadth first or Depth first</param>
        /// <returns></returns>
        public static DependencyObject Find(this DependencyObject root, Predicate<DependencyObject> search,
            GraphSearch graphSearchType = GraphSearch.BreadthFirst)
        {
            switch (graphSearchType)
            {
                case GraphSearch.DepthFirst:
                    return root.FindChildDepthFirst(search);
                case GraphSearch.BreadthFirst:
                    return root.FindChildBreadthFirst(search);
                default:
                    throw new ArgumentOutOfRangeException("graphSearchType", graphSearchType, null);
            }
        }

        private static DependencyObject FindChildDepthFirst(this DependencyObject root,
            Predicate<DependencyObject> isMatchPredicate)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);

                var result = isMatchPredicate(child) ? child : FindChildDepthFirst(child, isMatchPredicate);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        ///     Finds the first parent element from visual tree of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        public static T FindVisualAncestor<T>(this UIElement element) where T : class
        {
            while (element != null && !(element is T))
            {
                element = (UIElement)VisualTreeHelper.GetParent(element);
            }

            return element as T;
        }

        private static DependencyObject FindChildBreadthFirst(this DependencyObject root,
            Predicate<DependencyObject> isMatchPredicate)
        {
            var s = new List<DependencyObject>();
            var q = new Queue<DependencyObject>();

            s.Add(root);
            q.Enqueue(root);

            while (q.Any())
            {
                var current = q.Dequeue();

                if (isMatchPredicate(current))
                {
                    return current;
                }

                var count = VisualTreeHelper.GetChildrenCount(current);
                for (var i = 0; i < count; i++)
                {
                    var n = VisualTreeHelper.GetChild(current, i);
                    if (!s.Contains(n))
                    {
                        s.Add(n);
                        q.Enqueue(n);
                    }
                }
            }

            return null;
        }


        public static IEnumerable<DependencyObject> FindAll(this DependencyObject root,
            Predicate<DependencyObject> search)
        {
            return root.FindChildrenBreadthFirst(search);
        }


        private static IEnumerable<DependencyObject> FindChildrenBreadthFirst(this DependencyObject root,
            Predicate<DependencyObject> isMatchPredicate)
        {
            var s = new List<DependencyObject>();
            var q = new Queue<DependencyObject>();

            s.Add(root);
            q.Enqueue(root);

            while (q.Any())
            {
                var current = q.Dequeue();

                if (isMatchPredicate(current))
                {
                    yield return current;
                }

                var count = VisualTreeHelper.GetChildrenCount(current);
                for (var i = 0; i < count; i++)
                {
                    var n = VisualTreeHelper.GetChild(current, i);
                    if (!s.Contains(n))
                    {
                        s.Add(n);
                        q.Enqueue(n);
                    }
                }
            }
        }
    }
}
