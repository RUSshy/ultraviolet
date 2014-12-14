﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace TwistedLogik.Ultraviolet.Layout.Elements
{
    partial class VisualStateGroupCollection
    {
        /// <inheritdoc/>
        public Dictionary<String, VisualStateGroup>.Enumerator GetEnumerator()
        {
            return groups.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<String, VisualStateGroup>> IEnumerable<KeyValuePair<String, VisualStateGroup>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
