# Supported date and time formats

Date formats:

- "yyyyMMdd[-|T|_]HHmmss" (if timezone offset hint provided),
- "yyyyMMdd[-|T|_]HHmmssZ",
- "yyyyMMdd[-|T|_]HHmmss[+|-]HH",
- "yyyyMMdd[-|T|_]HHmmss[+|-]HHmm",

Examples:

- valid: Prefix_YYYYMMDD_hhmmss.wav, Prefix_YYYYMMDD_hhmmssZ.wav
- valid: prefix_20140101_235959.mp3, a_00000000_000000.a, a_99999999_999999.dnsb48364JSFDSD, prefix_20140101_235959Z.mp3
- valid: SERF_20130314_000021_000.wav, a_20130314_000021_a.a, a_99999999_999999_a.dnsb48364JSFDSD
- valid: prefix_20140101-235959+10.mp3, a_00000000-000000+00.a, a_99999999-999999+9999.dnsb48364JSFDSD
- valid: prefix_20140101_235959+10.mp3, a_00000000_000000+00.a, a_99999999_999999+9999.dnsb48364JSFDSD
- ISO8601-ish (supports a file compatible variant of ISO8601)
- valid: prefix_20140101T235959+10.mp3, a_00000000T000000+00.a, a_99999999T999999+9999.dnsb48364JSFDSD


## Problems with dates

- There is one true date format: ISO8601
- Dates and times can be ambiguous
- Time zones are silly
    - Daylight savings (e.g. AEDT)
    - Time zones change regularly
    - E.g. Sudan changed time zone from UTC +3 to UTC +2 on 1 November 2017
- Time zone offsets are very important
    - How many hours from UTC
    - Less problems than time zones


|    Bad Example                    |    Problem                                                   |    Solution                                                      |    Better Example                    |   |
|-----------------------------------|--------------------------------------------------------------|------------------------------------------------------------------|--------------------------------------|---|
|    3:45                           |    Can   occur twice a day                                   |    Use   24 hour time HH:mm                                      |    03:45                             |   |
|    3/3/2018                       |    Month   can come first                                    |    Use   ISO8601 dates                                           |    2018-03-03                        |   |
|    16:30   03/03/2018             |    Not   sortable                                            |    Order   date components from largest to smallest (ISO8601)    |    2018-03-03   03:45:00             |   |
|    2018-03-03   03:45:00          |    Could   refer to 37   different   points in time today    |    Always   use a time zone offset                               |    2018-03-03   03:45:00+10:00       |   |
|    592D42A3                       |    Unreadable      (…but   not ambiguous)                    |    Make   it readable                                            |    20180303T034500Z                  |   |
|    2018-03-03   03:45:00+10:00    |    Invalid   filename   characters                           |    ISO8601   alternative format                                  |    20180303T034500+1000              |   |