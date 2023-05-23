﻿using System.Collections.Generic;

namespace Simple.Logging.Configuration;
public abstract class ConfigObserverBase
{
    public abstract void ApplyOptions(Dictionary<string, string> options);
}