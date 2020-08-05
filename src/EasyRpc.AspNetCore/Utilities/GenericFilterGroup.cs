﻿using System;
using System.Collections.Generic;
using System.Text;
using EasyRpc.AspNetCore.Data;

namespace EasyRpc.AspNetCore.Utilities
{
    /// <summary>
    /// Generic filter group 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericFilterGroup<T>
    {
        private ImmutableLinkedList<Func<T, bool>> _typeFilters;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="typeFilters"></param>
        public GenericFilterGroup(params Func<T, bool>[] typeFilters)
        {
            if (typeFilters == null) throw new ArgumentNullException(nameof(typeFilters));

            _typeFilters = typeFilters.Length == 0 ?
                           ImmutableLinkedList<Func<T, bool>>.Empty :
                           ImmutableLinkedList.From(typeFilters);
        }

        /// <summary>
        /// Or together the filters rather than And them
        /// </summary>
        public bool UseOr { get; set; }

        /// <summary>
        /// Add filter to filter group
        /// </summary>
        /// <param name="filter"></param>
        public void Add(Func<T, bool> filter)
        {
            _typeFilters = _typeFilters.Add(filter);
        }

        /// <summary>
        /// Automatically convert from GenericFilterGroup to Func(T,bool)
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static implicit operator Func<T, bool>(GenericFilterGroup<T> group)
        {
            return group.Execute;
        }

        /// <summary>
        /// Internal method that does the filtering 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool Execute(T type)
        {
            if (UseOr)
            {
                foreach (var typeFilter in _typeFilters)
                {
                    if (typeFilter(type))
                    {
                        return true;
                    }
                }

                return _typeFilters == ImmutableLinkedList<Func<T, bool>>.Empty;
            }

            foreach (var typeFilter in _typeFilters)
            {
                if (!typeFilter(type))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
