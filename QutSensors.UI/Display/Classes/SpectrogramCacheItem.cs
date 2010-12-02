using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QutSensors.Shared;

/// <summary>
/// Summary description for SpectrogramCacheItem
/// </summary>
public class SpectrogramCacheItem
{
    public string AudioReadingIdQs { get; set; }

    public int CacheId { get; set; }

    public string LastAccessed { get; set; }

    public string Start { get; set; }

    public string End { get; set; }

    public string CacheType { get; set; }

    public string Duration { get; set; }
}