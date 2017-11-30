#!/usr/bin/python
from datetime import timedelta, date
import datetime

#creates a text file with the date and the number of days that went by since the start date of the experiment
with open('/Volumes/Nifty/QUT/Scripts/datelist.txt', 'w') as file:
	d = date(2015,06,22)
	delta = datetime.timedelta(days=1)
	number = 1
	while d <= date(2016,07,23):
	    file.write(d.strftime("%Y-%m-%d") + ' ' + str(number) + '\n')
	    d += delta
	    number += 1