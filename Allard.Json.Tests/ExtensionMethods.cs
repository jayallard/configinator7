﻿using Newtonsoft.Json.Linq;

namespace Allard.Json.Tests;

public static class ExtensionMethods
{
    public static JObject ToJObject(this object obj) => JObject.FromObject(obj);
}