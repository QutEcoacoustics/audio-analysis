#!/usr/bin/python
import sys
import csv
from datetime import timedelta, date
import datetime
import re
	
#creates one long sequence with all the clusters per minute for one location starting at 0:00 22-06
def createclusterlist(location):
	#open csv file with cluster per min and create one long sequence of the clusters
	with open('/Volumes/Nifty/QUT/Yvonne_AcousticData_WoondumAndGympie/ClusterMinute/Cluster50Sequence_'+location+'2015June22.csv', 'rb') as csvfile:
		clusterminute = csv.reader(csvfile, dialect=csv.excel_tab, delimiter = ',')
		clusterlist = []
		for row in clusterminute:
			cluster = row[0].split(',')[0]
			clusterlist.append(cluster)
		replacedic = {'1':'01','2':'02','3':'03','4':'04','5':'05','6':'06','7':'07','8':'08','9':'09'}
		clusterlistnew = []
		for x in clusterlist:
			if x in replacedic:
				clusterlistnew.append(replacedic[x])
			else:
				clusterlistnew.append(x)
		return clusterlistnew[1:]

#creates a sequence in string format of the sequence given in the list
def createclusterseq(clusterlist):
	clusterseq = ''
	for x in clusterlist:
		clusterseq += x
	return clusterseq

#change the symbols in the list
def altersymbols(clusterlist):
	# add +range(32,41) to newsymbols for 60 clusters
	newsymbols = list(map(chr, range(65,91)+range(97,123)))
	oldsymbols = list(range(0,61))
	newlist = []
	for x in clusterlist:
		if x == 'NA':
			newlist.append('-')
		else:
			x = int(x)
			if x==oldsymbols[x]:
				newlist.append(newsymbols[x])
	newseq = ''
	for x in newlist:
		newseq += x	
	return newseq


# creates a dictionary with date 'yyyy-mm-dd' as key and civil dawn time as value
def civildawndict(year,location):
	civiltwilight_f = open('/Volumes/Nifty/QUT/Yvonne_AcousticData_WoondumAndGympie/Other data/Misc/civiltwilight'+location+str(year)+'.txt','r')
	civiltwilight = civiltwilight_f.readlines()
	civildawn = {}
	x = 2
	y = 2
	#iterates through the year per month
	for i in range(1,13):
		start_date = date(year,i,1)
		if i<12:
			end_date = date(year,i+1,1)
		else:
			end_date = date(year+1,1,1)
		#top half of the text document is the first half year
		if i<=6:
			counter = 0
			#reads as many lines as there are days in the month
			for line in civiltwilight[:((end_date-start_date).days)]:
				line = line.split(' ')
				#keeps track of the current date per iteration
				datum = str(start_date + timedelta(days=counter))
				#per month the civildawn time is in a different column
				civildawn[datum] = line[x]
				counter += 1
			x+=5
		#bottom half of the text document is the last half year
		elif i>6:
			counter = 0
			for line in civiltwilight[31:31+((end_date-start_date).days)]:
				line = line.split(' ')
				datum = str(start_date + timedelta(days=counter))
				civildawn[datum] = line[y]
				counter += 1
			y+=5
	return civildawn

#creates a dictionary that has the day and time of civil dawn as key and the according sequence as value
def clusterseqpcivildawn(clusterseq,dict):
	#create list of time sequence of minutes from 22-06-2015 until 23-07-2016
	timeseq = createtimeseq(2015,06,22,2016,07,24,'minute')[:-1]
	#create dictionary that has time in sequence as key and position as value
	dicttimeseq = {time:position for position,time in enumerate(timeseq)}
	#look up position of civil dawn time by using civil dawn time as key in the dictionary just made
	#first we have to make a list of the civil dawn times we can iterate through
	listcivildawn = []
	for key in civildawndict:
		listcivildawn.append(key+'-'+civildawndict[key])

	#create list of positions in timeseq with the time of civil dawn
	poscivildawn = []
	#create dictionary of positions in timeseq as key and date/time as value
	dictposdate = {}
	for civildawn in listcivildawn:
		if civildawn in dicttimeseq:
			poscivildawn.append(dicttimeseq[civildawn])
			dictposdate[dicttimeseq[civildawn]] = civildawn
	#sort the list on positions
	poscivildawn.sort(key = int)
	
	#create dictionary that contains the clustersequence with starting at civil dawn
	seqpcivildawndict = {}
	for i in range(len(poscivildawn)):
		if i<(len(poscivildawn)-1):
			begin = poscivildawn[i]
			end = poscivildawn[i+1]
			if symbols == 'numbers':
				seqcivildawn = clusterseq[(begin*2):(end*2)]
			else:
				seqcivildawn = clusterseq[begin:end]
			date = dictposdate[begin]
		else:
			if symbols == 'numbers':
				seqcivildawn = clusterseq[(end*2):]
			else:
				seqcivildawn = clusterseq[end:]
			date = dictposdate[poscivildawn[i]]
		seqpcivildawndict[date] = seqcivildawn
	return seqpcivildawndict

#creates a list that exists of a sequence of date/time moments, scale can be given in days or minutes
#start_date and end_date in form of: yyyy,mm,dd
def createtimeseq(syyyy,smm,sdd, eyyyy,emm,edd, scale):
	current_date = datetime.datetime(syyyy,smm,sdd)
	no_days = len(range(int((date(eyyyy,emm,edd)-date(syyyy,smm,sdd)).days)))
	end= current_date + datetime.timedelta(days=no_days)
	l=[]
	date_list = []
	while current_date <= end:
		l.append(current_date)
		if scale == 'day':
			date_list.append(current_date.strftime("%Y-%m-%d"))
			current_date += datetime.timedelta(days=1)
		
		elif scale == 'minute':
			date_list.append(current_date.strftime("%Y-%m-%d-%H%M"))
			current_date += datetime.timedelta(minutes=1)

	return date_list

#creates a dictionary of sequence per day from 0:00 until 0:00
def clusterseqpday(clusterseq):
	#one day is 1440 minutes
	if symbols == 'letters':
		stepsize = 1440
	elif symbols == 'numbers':
		stepsize = 2880
	j = stepsize
	k = 0
	seqpdaydict = {}
	date_list = createtimeseq(2015,6,22,2016,7,23,'day')
	for i in range(0,len(clusterseq),stepsize):
		day = str(date_list[k])
		seqpdaydict[day] = clusterseq[i:j]
		i += stepsize
		j += stepsize
		k += 1
	return seqpdaydict

#Writes the dictionary with sequences as value in a fasta file format
def writefasta(seqdict, location,startpoint):
	with open('/Volumes/Nifty/QUT/Scripts/'+location+'_'+symbols+'_'+startpoint+'.txt', 'w') as file:
		for day in sorted(seqdict):
			seq = seqdict[day]
			seq = "\n".join(re.findall("(?s).{,70}", seq))[:-1]
			description = startpoint
			file.write('>' +location + '|' + day + '|'+ description +'\n')
			file.write(seq + '\n')
	file.close()

if len(sys.argv)!=4:
	print 'Function need three arguments: location, symbols and startpoint'
	exit()
else:
	location = sys.argv[1]
	symbols = sys.argv[2]
	startpoint = sys.argv[3]

#1 Location is Gympie or Woondum with capital!!
if location in ['Gympie','Woondum']:
	clusterlist = createclusterlist(location)
else:
	print 'Location should be either \'Gympie\' or \'Woondum\''
	exit()


#2 Symbols is either 'numbers or letters'
if symbols == 'numbers':
	clusterseq = createclusterseq(clusterlist)
elif symbols =='letters':
	clusterseq = altersymbols(clusterlist)
else:
	print 'Symbols should be either \'numbers\' or \'letters\''
	exit()

#3 Startpoint is either civildawn or midnight
if startpoint == 'civildawn':
	civildawndict2015 = civildawndict(2015,location)
	civildawndict2016 = civildawndict(2016,location)
	civildawndict = dict(civildawndict2015,**civildawndict2016)
	seqdict= clusterseqpcivildawn(clusterseq,civildawndict)
elif startpoint == 'midnight':
	seqdict = clusterseqpday(clusterseq)
else:
	print 'Startpoint should be either \'civildawn\' or \'midnight\''
	exit()

#4 File name
writefasta(seqdict,location,startpoint)