#################################################################
# This code reads in separate 1 minute wave files and calculates 
# spectral properties using seewave

#################################################################
library(seewave)
library(tuneR)
setwd("C:\\Work\\Github\\audio-analysis\\AudioAnalysis\\RCode\\Yvonne")

sourceDir <- "C:\\Work\\Output"

myFiles <- list.files(path=sourceDir, full.names=TRUE,
                      pattern="*.wav")
source("..\\shared\\sort.Filename.R")#changed to sort.filenames

myFiles <- sort.Filename(myFiles)# changed to sort.filenames

fileCount <- length(myFiles)

# Spectral properties function
getSpectralProperties <- function(file){
                print("starting file")
                wavFile <- readWave(file)               
                meanSp <- meanspec(wavFile, f=22050,plot=FALSE)
                result.prop <- specprop(meanSp)
                return(result.prop)
}

spectralProperties <- sapply(myFiles[1:60], getSpectralProperties,
                             USE.NAMES=FALSE)
spectralProperties <- aperm(spectralProperties) # transpose array

View(spectralProperties)

#Acoustic complexity index function
getACI <- function(file){
        print("starting file")
        wavFile <- readWave(file)               
        result.aci<-ACI(wavFile)
        return(result.aci)
}

acousticCompIndex <- sapply(myFiles[1:60], getACI,
                             USE.NAMES=FALSE)

View(acousticCompIndex)

#Zero crossing rate
getZCR <- function(file){
        print("starting file")
        wavFile <- readWave(file)               
        result.zcr<-zcr(wavFile,plot=F,wl=NULL)
        return(result.zcr)
}

zeroCrossingRate <- sapply(myFiles[1:60], getZCR,
                            USE.NAMES=FALSE)

View(zeroCrossingRate)

#Temporal entropy
getTempEntropy <- function(file){
        print("starting file")
        wavFile <- readWave(file) 
        envb<-env(wavFile,f=22050,plot=FALSE)
        result.th<-th(envb)
        return(result.th)
}

temporalEntropy <- sapply(myFiles[1:60], getTempEntropy,
                           USE.NAMES=FALSE)

View(temporalEntropy)

allProperties<-cbind(acousticCompIndex, zeroCrossingRate, 
                     temporalEntropy,spectralProperties)

View(allProperties)

#wav.1<-readWave(myfiles[1])
#wav.2<-readWave(myfiles[2])
#wav.3<-readWave(myfiles[3])
#wav.4<-readWave(myfiles[4])
#wav.5<-readWave(myfiles[5])
#wav.6<-readWave(myfiles[6])
#wav.7<-readWave(myfiles[7])
#wav.8<-readWave(myfiles[8])
#wav.9<-readWave(myfiles[9])
#wav.10<-readWave(myfiles[10])
#wav.11<-readWave(myfiles[11])
#wav.12<-readWave(myfiles[12])
#wav.13<-readWave(myfiles[13])
#wav.14<-readWave(myfiles[14])
#wav.15<-readWave(myfiles[15])
#wav.16<-readWave(myfiles[16])
#wav.17<-readWave(myfiles[17])
#wav.18<-readWave(myfiles[18])
#wav.19<-readWave(myfiles[19])
#wav.20<-readWave(myfiles[20])
#wav.21<-readWave(myfiles[21])
#wav.22<-readWave(myfiles[22])
#wav.23<-readWave(myfiles[23])
#wav.24<-readWave(myfiles[24])
#wav.25<-readWave(myfiles[25])
#wav.26<-readWave(myfiles[26])
#wav.27<-readWave(myfiles[27])
#wav.28<-readWave(myfiles[28])
#wav.29<-readWave(myfiles[29])
#wav.30<-readWave(myfiles[30])
#wav.31<-readWave(myfiles[31])
#wav.32<-readWave(myfiles[32])
#wav.33<-readWave(myfiles[33])
#wav.34<-readWave(myfiles[34])
#wav.35<-readWave(myfiles[35])
#wav.36<-readWave(myfiles[36])
#wav.37<-readWave(myfiles[37])
#wav.38<-readWave(myfiles[38])
#wav.39<-readWave(myfiles[39])
#wav.40<-readWave(myfiles[40])
#wav.41<-readWave(myfiles[41])
#wav.42<-readWave(myfiles[42])
#wav.43<-readWave(myfiles[43])
#wav.44<-readWave(myfiles[44])
#wav.45<-readWave(myfiles[45])
#wav.46<-readWave(myfiles[46])
#wav.47<-readWave(myfiles[47])
#wav.48<-readWave(myfiles[48])
#wav.49<-readWave(myfiles[49])
#wav.50<-readWave(myfiles[50])
#wav.51<-readWave(myfiles[51])
#wav.52<-readWave(myfiles[52])
#wav.53<-readWave(myfiles[53])
#wav.54<-readWave(myfiles[54])
#wav.55<-readWave(myfiles[55])
#wav.56<-readWave(myfiles[56])
#wav.57<-readWave(myfiles[57])
#wav.58<-readWave(myfiles[58])
#wav.59<-readWave(myfiles[59])
#wav.60<-readWave(myfiles[60])

wav.1.1 <- cutw(wav.1, from=0.0, to=60, output="Wave")
wav.1.2 <- cutw(wav.1, from=60, to=120, output="Wave")
wav.1.3 <- cutw(wav.1, from=120, to=180, output="Wave")
wav.1.4 <- cutw(wav.1, from=180, to=240, output="Wave")
wav.1.5 <- cutw(wav.1, from=240, to=300, output="Wave")
wav.1.6 <- cutw(wav.1, from=300, to=360, output="Wave")
wav.1.7 <- cutw(wav.1, from=360, to=420, output="Wave")
wav.1.8 <- cutw(wav.1, from=420, to=480, output="Wave")
wav.1.9 <- cutw(wav.1, from=480, to=540, output="Wave")
wav.1.10 <- cutw(wav.1, from=540, to=600, output="Wave")
wav.1.11 <- cutw(wav.1, from=600, to=660, output="Wave")
wav.1.12 <- cutw(wav.1, from=660, to=720, output="Wave")
wav.1.13 <- cutw(wav.1, from=720, to=780, output="Wave")
wav.1.14 <- cutw(wav.1, from=780, to=840, output="Wave")
wav.1.15 <- cutw(wav.1, from=840, to=900, output="Wave")
wav.1.16 <- cutw(wav.1, from=900, to=960, output="Wave")
wav.1.17 <- cutw(wav.1, from=960, to=1020, output="Wave")
wav.1.18 <- cutw(wav.1, from=1020, to=1080, output="Wave")
wav.1.19 <- cutw(wav.1, from=1080, to=1140, output="Wave")
wav.1.20 <- cutw(wav.1, from=1140, to=1200, output="Wave")
wav.1.21 <- cutw(wav.1, from=1200, to=1260, output="Wave")
wav.1.22 <- cutw(wav.1, from=1260, to=1320, output="Wave")
wav.1.23 <- cutw(wav.1, from=1320, to=1380, output="Wave")
wav.1.24 <- cutw(wav.1, from=1380, to=1440, output="Wave")
wav.1.25 <- cutw(wav.1, from=1440, to=1500, output="Wave")
wav.1.26 <- cutw(wav.1, from=1500, to=1560, output="Wave")
wav.1.27 <- cutw(wav.1, from=1560, to=1620, output="Wave")
wav.1.28 <- cutw(wav.1, from=1620, to=1680, output="Wave")
wav.1.29 <- cutw(wav.1, from=1680, to=1740, output="Wave")
wav.1.30 <- cutw(wav.1, from=1740, to=1800, output="Wave")
wav.1.31 <- cutw(wav.1, from=1800, to=1860, output="Wave")
wav.1.32 <- cutw(wav.1, from=1860, to=1920, output="Wave")
wav.1.33 <- cutw(wav.1, from=1920, to=1980, output="Wave")
wav.1.34 <- cutw(wav.1, from=1980, to=2040, output="Wave")
wav.1.35 <- cutw(wav.1, from=2040, to=2100, output="Wave")
wav.1.36 <- cutw(wav.1, from=2100, to=2160, output="Wave")
wav.1.37 <- cutw(wav.1, from=2160, to=2220, output="Wave")
wav.1.38 <- cutw(wav.1, from=2220, to=2280, output="Wave")
wav.1.39 <- cutw(wav.1, from=2280, to=2340, output="Wave")
wav.1.40 <- cutw(wav.1, from=2340, to=2400, output="Wave")
wav.1.41 <- cutw(wav.1, from=2400, to=2460, output="Wave")
wav.1.42 <- cutw(wav.1, from=2460, to=2520, output="Wave")
wav.1.43 <- cutw(wav.1, from=2520, to=2580, output="Wave")
wav.1.44 <- cutw(wav.1, from=2580, to=2640, output="Wave")
wav.1.45 <- cutw(wav.1, from=2640, to=2700, output="Wave")
wav.1.46 <- cutw(wav.1, from=2700, to=2760, output="Wave")
wav.1.47 <- cutw(wav.1, from=2760, to=2820, output="Wave")
wav.1.48 <- cutw(wav.1, from=2820, to=2880, output="Wave")
wav.1.49 <- cutw(wav.1, from=2880, to=2940, output="Wave")
wav.1.50 <- cutw(wav.1, from=2940, to=3000, output="Wave")
wav.1.51 <- cutw(wav.1, from=3000, to=3060, output="Wave")
wav.1.52 <- cutw(wav.1, from=3060, to=3120, output="Wave")
wav.1.53 <- cutw(wav.1, from=3120, to=3180, output="Wave")
wav.1.54 <- cutw(wav.1, from=3180, to=3240, output="Wave")
wav.1.55 <- cutw(wav.1, from=3240, to=3300, output="Wave")
wav.1.56 <- cutw(wav.1, from=3300, to=3360, output="Wave")
wav.1.57 <- cutw(wav.1, from=3360, to=3420, output="Wave")
wav.1.58 <- cutw(wav.1, from=3420, to=3480, output="Wave")
wav.1.59 <- cutw(wav.1, from=3480, to=3540, output="Wave")
wav.1.60 <- cutw(wav.1, from=3540, to=3600, output="Wave")

a<-meanspec(wav.1.1,f=22050, plot=FALSE)
b<-meanspec(wav.1.2,f=22050, plot=FALSE)
c<-meanspec(wav.1.3,f=22050, plot=FALSE)
d<-meanspec(wav.1.4,f=22050, plot=FALSE)
e<-meanspec(wav.1.5,f=22050, plot=FALSE)
f<-meanspec(wav.1.6,f=22050, plot=FALSE)
g<-meanspec(wav.1.7,f=22050, plot=FALSE)
h<-meanspec(wav.1.8,f=22050, plot=FALSE)
i<-meanspec(wav.1.9,f=22050, plot=FALSE)
j<-meanspec(wav.1.10,f=22050, plot=FALSE)
k<-meanspec(wav.1.11,f=22050, plot=FALSE)
l<-meanspec(wav.1.12,f=22050, plot=FALSE)
m<-meanspec(wav.1.13,f=22050, plot=FALSE)
n<-meanspec(wav.1.14,f=22050, plot=FALSE)
o<-meanspec(wav.1.15,f=22050, plot=FALSE)
p<-meanspec(wav.1.16,f=22050, plot=FALSE)
q<-meanspec(wav.1.17,f=22050, plot=FALSE)
r<-meanspec(wav.1.18,f=22050, plot=FALSE)
s<-meanspec(wav.1.19,f=22050, plot=FALSE)
t<-meanspec(wav.1.20,f=22050, plot=FALSE)
u<-meanspec(wav.1.21,f=22050, plot=FALSE)
v<-meanspec(wav.1.22,f=22050, plot=FALSE)
w<-meanspec(wav.1.23,f=22050, plot=FALSE)
x<-meanspec(wav.1.24,f=22050, plot=FALSE)
y<-meanspec(wav.1.25,f=22050, plot=FALSE)
z<-meanspec(wav.1.26,f=22050, plot=FALSE)
A<-meanspec(wav.1.27,f=22050, plot=FALSE)
B<-meanspec(wav.1.28,f=22050, plot=FALSE)
C<-meanspec(wav.1.29,f=22050, plot=FALSE)
D<-meanspec(wav.1.30,f=22050, plot=FALSE)
E<-meanspec(wav.1.31,f=22050, plot=FALSE)
F<-meanspec(wav.1.32,f=22050, plot=FALSE)
G<-meanspec(wav.1.33,f=22050, plot=FALSE)
H<-meanspec(wav.1.34,f=22050, plot=FALSE)
I<-meanspec(wav.1.35,f=22050, plot=FALSE)
J<-meanspec(wav.1.36,f=22050, plot=FALSE)
K<-meanspec(wav.1.37,f=22050, plot=FALSE)
L<-meanspec(wav.1.38,f=22050, plot=FALSE)
M<-meanspec(wav.1.39,f=22050, plot=FALSE)
N<-meanspec(wav.1.40,f=22050, plot=FALSE)
O<-meanspec(wav.1.41,f=22050, plot=FALSE)
P<-meanspec(wav.1.42,f=22050, plot=FALSE)
Q<-meanspec(wav.1.43,f=22050, plot=FALSE)
R<-meanspec(wav.1.44,f=22050, plot=FALSE)
S<-meanspec(wav.1.45,f=22050, plot=FALSE)
T<-meanspec(wav.1.46,f=22050, plot=FALSE)
U<-meanspec(wav.1.47,f=22050, plot=FALSE)
V<-meanspec(wav.1.48,f=22050, plot=FALSE)
W<-meanspec(wav.1.49,f=22050, plot=FALSE)
X<-meanspec(wav.1.50,f=22050, plot=FALSE)
Y<-meanspec(wav.1.51,f=22050, plot=FALSE)
Z<-meanspec(wav.1.52,f=22050, plot=FALSE)
aa<-meanspec(wav.1.53,f=22050, plot=FALSE)
ab<-meanspec(wav.1.54,f=22050, plot=FALSE)
ac<-meanspec(wav.1.55,f=22050, plot=FALSE)
ad<-meanspec(wav.1.56,f=22050, plot=FALSE)
ae<-meanspec(wav.1.57,f=22050, plot=FALSE)
af<-meanspec(wav.1.58,f=22050, plot=FALSE)
ag<-meanspec(wav.1.59,f=22050, plot=FALSE)
ah<-meanspec(wav.1.60,f=22050, plot=FALSE)

result.a<-specprop(a)
result.b<-specprop(b)
result.c<-specprop(c)
result.d<-specprop(d)
result.e<-specprop(e)
result.f<-specprop(f)
result.g<-specprop(g)
result.h<-specprop(h)
result.i<-specprop(i)
result.j<-specprop(j)
result.k<-specprop(k)
result.l<-specprop(l)
result.m<-specprop(m)
result.n<-specprop(n)
result.o<-specprop(o)
result.p<-specprop(p)
result.q<-specprop(q)
result.r<-specprop(r)
result.s<-specprop(s)
result.t<-specprop(t)
result.u<-specprop(u)
result.v<-specprop(v)
result.w<-specprop(w)
result.x<-specprop(x)
result.y<-specprop(y)
result.z<-specprop(z)
result.A<-specprop(A)
result.B<-specprop(B)
result.C<-specprop(C)
result.D<-specprop(D)
result.E<-specprop(E)
result.F<-specprop(F)
result.G<-specprop(G)
result.H<-specprop(H)
result.I<-specprop(I)
result.J<-specprop(J)
result.K<-specprop(K)
result.L<-specprop(L)
result.M<-specprop(M)
result.N<-specprop(N)
result.O<-specprop(O)
result.P<-specprop(P)
result.Q<-specprop(Q)
result.R<-specprop(R)
result.S<-specprop(S)
result.T<-specprop(T)
result.U<-specprop(U)
result.V<-specprop(V)
result.W<-specprop(W)
result.X<-specprop(X)
result.Y<-specprop(Y)
result.Z<-specprop(Z)
result.aa<-specprop(aa)
result.ab<-specprop(ab)
result.ac<-specprop(ac)
result.ad<-specprop(ad)
result.ae<-specprop(ae)
result.af<-specprop(af)
result.ag<-specprop(ag)
result.ah<-specprop(ah)

result<-rbind(result.a,result.b,result.c,
              result.d,result.e,result.f,
              result.g,result.h,result.i,
              result.j,result.k,result.l,
              result.m,result.n,result.o,
              result.p,result.q,result.r,
              result.s,result.t,result.u,
              result.v,result.w,result.x,
              result.y,result.z,result.A,
              result.B,result.C,result.D,
              result.E,result.F,result.G,
              result.H,result.I,result.J,
              result.K,result.L,result.M,
              result.N,result.O,result.P,
              result.Q,result.R,result.S,
              result.T,result.U,result.V,
              result.W,result.X,result.Y,
              result.Z,result.aa,result.ab,
              result.ac,result.ad,result.ae,
              result.af,result.ag,result.ah)
View(result)
#Acoustic Complexity Index
result.aci.a<-ACI(wav.1.1)
result.aci.b<-ACI(wav.1.2)
result.aci.c<-ACI(wav.1.3)
result.aci.d<-ACI(wav.1.4)
result.aci.e<-ACI(wav.1.5)
result.aci.f<-ACI(wav.1.6)
result.aci.g<-ACI(wav.1.7)
result.aci.h<-ACI(wav.1.8)
result.aci.i<-ACI(wav.1.9)
result.aci.j<-ACI(wav.1.10)
result.aci.k<-ACI(wav.1.11)
result.aci.l<-ACI(wav.1.12)
result.aci.m<-ACI(wav.1.13)
result.aci.n<-ACI(wav.1.14)
result.aci.o<-ACI(wav.1.15)
result.aci.p<-ACI(wav.1.16)
result.aci.q<-ACI(wav.1.17)
result.aci.r<-ACI(wav.1.18)
result.aci.s<-ACI(wav.1.19)
result.aci.t<-ACI(wav.1.20)
result.aci.u<-ACI(wav.1.21)
result.aci.v<-ACI(wav.1.22)
result.aci.w<-ACI(wav.1.23)
result.aci.x<-ACI(wav.1.24)
result.aci.y<-ACI(wav.1.25)
result.aci.z<-ACI(wav.1.26)
result.aci.A<-ACI(wav.1.27)
result.aci.B<-ACI(wav.1.28)
result.aci.C<-ACI(wav.1.29)
result.aci.D<-ACI(wav.1.30)
result.aci.E<-ACI(wav.1.31)
result.aci.F<-ACI(wav.1.32)
result.aci.G<-ACI(wav.1.33)
result.aci.H<-ACI(wav.1.34)
result.aci.I<-ACI(wav.1.35)
result.aci.J<-ACI(wav.1.36)
result.aci.K<-ACI(wav.1.37)
result.aci.L<-ACI(wav.1.38)
result.aci.M<-ACI(wav.1.39)
result.aci.N<-ACI(wav.1.40)
result.aci.O<-ACI(wav.1.41)
result.aci.P<-ACI(wav.1.42)
result.aci.Q<-ACI(wav.1.43)
result.aci.R<-ACI(wav.1.44)
result.aci.S<-ACI(wav.1.45)
result.aci.T<-ACI(wav.1.46)
result.aci.U<-ACI(wav.1.47)
result.aci.V<-ACI(wav.1.48)
result.aci.W<-ACI(wav.1.49)
result.aci.X<-ACI(wav.1.50)
result.aci.Y<-ACI(wav.1.51)
result.aci.Z<-ACI(wav.1.52)
result.aci.aa<-ACI(wav.1.53)
result.aci.ab<-ACI(wav.1.54)
result.aci.ac<-ACI(wav.1.55)
result.aci.ad<-ACI(wav.1.56)
result.aci.ae<-ACI(wav.1.57)
result.aci.af<-ACI(wav.1.58)
result.aci.ag<-ACI(wav.1.59)
result.aci.ah<-ACI(wav.1.60)

aci.result<-rbind(result.aci.a,result.aci.b,result.aci.c,
                  result.aci.d,result.aci.e,result.aci.f,
                  result.aci.g,result.aci.h,result.aci.i,
                  result.aci.j,result.aci.k,result.aci.l,
                  result.aci.m,result.aci.n,result.aci.o,
                  result.aci.p,result.aci.q,result.aci.r,
                  result.aci.s,result.aci.t,result.aci.u,
                  result.aci.v,result.aci.w,result.aci.x,
                  result.aci.y,result.aci.z,result.aci.A,
                  result.aci.B,result.aci.C,result.aci.D,
                  result.aci.E,result.aci.F,result.aci.G,
                  result.aci.H,result.aci.I,result.aci.J,
                  result.aci.K,result.aci.L,result.aci.M,
                  result.aci.N,result.aci.O,result.aci.P,
                  result.aci.Q,result.aci.R,result.aci.S,
                  result.aci.T,result.aci.U,result.aci.V,
                  result.aci.W,result.aci.X,result.aci.Y,
                  result.aci.Z,result.aci.aa,result.aci.ab,
                  result.aci.ac,result.aci.ad,result.aci.ae,
                  result.aci.af,result.aci.ag,result.aci.ah)
View(aci.result)

#Zero Crossing rate
result.zcr.a<-zcr(wav.1.1, plot=FALSE, wl=NULL)
result.zcr.b<-zcr(wav.1.2, plot=FALSE, wl=NULL)
result.zcr.c<-zcr(wav.1.3, plot=FALSE, wl=NULL)
result.zcr.d<-zcr(wav.1.4, plot=FALSE, wl=NULL)
result.zcr.e<-zcr(wav.1.5, plot=FALSE, wl=NULL)
result.zcr.f<-zcr(wav.1.6, plot=FALSE, wl=NULL)
result.zcr.g<-zcr(wav.1.7, plot=FALSE, wl=NULL)
result.zcr.h<-zcr(wav.1.8, plot=FALSE, wl=NULL)
result.zcr.i<-zcr(wav.1.9, plot=FALSE, wl=NULL)
result.zcr.j<-zcr(wav.1.10, plot=FALSE, wl=NULL)
result.zcr.k<-zcr(wav.1.11, plot=FALSE, wl=NULL)
result.zcr.l<-zcr(wav.1.12, plot=FALSE, wl=NULL)
result.zcr.m<-zcr(wav.1.13, plot=FALSE, wl=NULL)
result.zcr.n<-zcr(wav.1.14, plot=FALSE, wl=NULL)
result.zcr.o<-zcr(wav.1.15, plot=FALSE, wl=NULL)
result.zcr.p<-zcr(wav.1.16, plot=FALSE, wl=NULL)
result.zcr.q<-zcr(wav.1.17, plot=FALSE, wl=NULL)
result.zcr.r<-zcr(wav.1.18, plot=FALSE, wl=NULL)
result.zcr.s<-zcr(wav.1.19, plot=FALSE, wl=NULL)
result.zcr.t<-zcr(wav.1.20, plot=FALSE, wl=NULL)
result.zcr.u<-zcr(wav.1.21, plot=FALSE, wl=NULL)
result.zcr.v<-zcr(wav.1.22, plot=FALSE, wl=NULL)
result.zcr.w<-zcr(wav.1.23, plot=FALSE, wl=NULL)
result.zcr.x<-zcr(wav.1.24, plot=FALSE, wl=NULL)
result.zcr.y<-zcr(wav.1.25, plot=FALSE, wl=NULL)
result.zcr.z<-zcr(wav.1.26, plot=FALSE, wl=NULL)
result.zcr.A<-zcr(wav.1.27, plot=FALSE, wl=NULL)
result.zcr.B<-zcr(wav.1.28, plot=FALSE, wl=NULL)
result.zcr.C<-zcr(wav.1.29, plot=FALSE, wl=NULL)
result.zcr.D<-zcr(wav.1.30, plot=FALSE, wl=NULL)
result.zcr.E<-zcr(wav.1.31, plot=FALSE, wl=NULL)
result.zcr.F<-zcr(wav.1.32, plot=FALSE, wl=NULL)
result.zcr.G<-zcr(wav.1.33, plot=FALSE, wl=NULL)
result.zcr.H<-zcr(wav.1.34, plot=FALSE, wl=NULL)
result.zcr.I<-zcr(wav.1.35, plot=FALSE, wl=NULL)
result.zcr.J<-zcr(wav.1.36, plot=FALSE, wl=NULL)
result.zcr.K<-zcr(wav.1.37, plot=FALSE, wl=NULL)
result.zcr.L<-zcr(wav.1.38, plot=FALSE, wl=NULL)
result.zcr.M<-zcr(wav.1.39, plot=FALSE, wl=NULL)
result.zcr.N<-zcr(wav.1.40, plot=FALSE, wl=NULL)
result.zcr.O<-zcr(wav.1.41, plot=FALSE, wl=NULL)
result.zcr.P<-zcr(wav.1.42, plot=FALSE, wl=NULL)
result.zcr.Q<-zcr(wav.1.43, plot=FALSE, wl=NULL)
result.zcr.R<-zcr(wav.1.44, plot=FALSE, wl=NULL)
result.zcr.S<-zcr(wav.1.45, plot=FALSE, wl=NULL)
result.zcr.T<-zcr(wav.1.46, plot=FALSE, wl=NULL)
result.zcr.U<-zcr(wav.1.47, plot=FALSE, wl=NULL)
result.zcr.V<-zcr(wav.1.48, plot=FALSE, wl=NULL)
result.zcr.W<-zcr(wav.1.49, plot=FALSE, wl=NULL)
result.zcr.X<-zcr(wav.1.50, plot=FALSE, wl=NULL)
result.zcr.Y<-zcr(wav.1.51, plot=FALSE, wl=NULL)
result.zcr.Z<-zcr(wav.1.52, plot=FALSE, wl=NULL)
result.zcr.aa<-zcr(wav.1.53, plot=FALSE, wl=NULL)
result.zcr.ab<-zcr(wav.1.54, plot=FALSE, wl=NULL)
result.zcr.ac<-zcr(wav.1.55, plot=FALSE, wl=NULL)
result.zcr.ad<-zcr(wav.1.56, plot=FALSE, wl=NULL)
result.zcr.ae<-zcr(wav.1.57, plot=FALSE, wl=NULL)
result.zcr.af<-zcr(wav.1.58, plot=FALSE, wl=NULL)
result.zcr.ag<-zcr(wav.1.59, plot=FALSE, wl=NULL)
result.zcr.ah<-zcr(wav.1.60, plot=FALSE, wl=NULL)

zcr.result<-rbind(result.zcr.a,result.zcr.b,result.zcr.c,
                  result.zcr.d,result.zcr.e,result.zcr.f,
                  result.zcr.g,result.zcr.h,result.zcr.i,
                  result.zcr.j,result.zcr.k,result.zcr.l,
                  result.zcr.m,result.zcr.n,result.zcr.o,
                  result.zcr.p,result.zcr.q,result.zcr.r,
                  result.zcr.s,result.zcr.t,result.zcr.u,
                  result.zcr.v,result.zcr.w,result.zcr.x,
                  result.zcr.y,result.zcr.z,result.zcr.A,
                  result.zcr.B,result.zcr.C,result.zcr.D,
                  result.zcr.E,result.zcr.F,result.zcr.G,
                  result.zcr.H,result.zcr.I,result.zcr.J,
                  result.zcr.K,result.zcr.L,result.zcr.M,
                  result.zcr.N,result.zcr.O,result.zcr.P,
                  result.zcr.Q,result.zcr.R,result.zcr.S,
                  result.zcr.T,result.zcr.U,result.zcr.V,
                  result.zcr.W,result.zcr.X,result.zcr.Y,
                  result.zcr.Z,result.zcr.aa,result.zcr.ab,
                  result.zcr.ac,result.zcr.ad,result.zcr.ae,
                  result.zcr.af,result.zcr.ag,result.zcr.ah)
View(zcr.result)

#temporal entropy
envwav.1.1<-env(wav.1.1,f=22050,plot=FALSE);result.th.a<-th(envwav.1.1)
envwav.1.2<-env(wav.1.1,f=22050,plot=FALSE);result.th.b<-th(envwav.1.1)
envwav.1.3<-env(wav.1.1,f=22050,plot=FALSE);result.th.c<-th(envwav.1.1)
envwav.1.4<-env(wav.1.1,f=22050,plot=FALSE);result.th.d<-th(envwav.1.1)
envwav.1.5<-env(wav.1.1,f=22050,plot=FALSE);result.th.e<-th(envwav.1.1)
envwav.1.6<-env(wav.1.1,f=22050,plot=FALSE);result.th.f<-th(envwav.1.1)
envwav.1.7<-env(wav.1.1,f=22050,plot=FALSE);result.th.g<-th(envwav.1.1)
envwav.1.8<-env(wav.1.1,f=22050,plot=FALSE);result.th.h<-th(envwav.1.1)
envwav.1.9<-env(wav.1.1,f=22050,plot=FALSE);result.th.i<-th(envwav.1.1)
envwav.1.10<-env(wav.1.1,f=22050,plot=FALSE);result.th.j<-th(envwav.1.1)
envwav.1.11<-env(wav.1.1,f=22050,plot=FALSE);result.th.k<-th(envwav.1.1)
envwav.1.12<-env(wav.1.1,f=22050,plot=FALSE);result.th.l<-th(envwav.1.1)
envwav.1.13<-env(wav.1.1,f=22050,plot=FALSE);result.th.m<-th(envwav.1.1)
envwav.1.14<-env(wav.1.1,f=22050,plot=FALSE);result.th.n<-th(envwav.1.1)
envwav.1.15<-env(wav.1.1,f=22050,plot=FALSE);result.th.o<-th(envwav.1.1)
envwav.1.16<-env(wav.1.1,f=22050,plot=FALSE);result.th.p<-th(envwav.1.1)
envwav.1.17<-env(wav.1.1,f=22050,plot=FALSE);result.th.q<-th(envwav.1.1)
envwav.1.18<-env(wav.1.1,f=22050,plot=FALSE);result.th.r<-th(envwav.1.1)
envwav.1.19<-env(wav.1.1,f=22050,plot=FALSE);result.th.s<-th(envwav.1.1)
envwav.1.20<-env(wav.1.1,f=22050,plot=FALSE);result.th.t<-th(envwav.1.1)
envwav.1.21<-env(wav.1.1,f=22050,plot=FALSE);result.th.u<-th(envwav.1.1)
envwav.1.22<-env(wav.1.1,f=22050,plot=FALSE);result.th.v<-th(envwav.1.1)
envwav.1.23<-env(wav.1.1,f=22050,plot=FALSE);result.th.w<-th(envwav.1.1)
envwav.1.24<-env(wav.1.1,f=22050,plot=FALSE);result.th.x<-th(envwav.1.1)
envwav.1.25<-env(wav.1.1,f=22050,plot=FALSE);result.th.y<-th(envwav.1.1)
envwav.1.26<-env(wav.1.1,f=22050,plot=FALSE);result.th.z<-th(envwav.1.1)
envwav.1.27<-env(wav.1.1,f=22050,plot=FALSE);result.th.A<-th(envwav.1.1)
envwav.1.28<-env(wav.1.1,f=22050,plot=FALSE);result.th.B<-th(envwav.1.1)
envwav.1.29<-env(wav.1.1,f=22050,plot=FALSE);result.th.C<-th(envwav.1.1)
envwav.1.30<-env(wav.1.1,f=22050,plot=FALSE);result.th.D<-th(envwav.1.1)
envwav.1.31<-env(wav.1.1,f=22050,plot=FALSE);result.th.E<-th(envwav.1.1)
envwav.1.32<-env(wav.1.1,f=22050,plot=FALSE);result.th.F<-th(envwav.1.1)
envwav.1.33<-env(wav.1.1,f=22050,plot=FALSE);result.th.G<-th(envwav.1.1)
envwav.1.34<-env(wav.1.1,f=22050,plot=FALSE);result.th.H<-th(envwav.1.1)
envwav.1.35<-env(wav.1.1,f=22050,plot=FALSE);result.th.I<-th(envwav.1.1)
envwav.1.36<-env(wav.1.1,f=22050,plot=FALSE);result.th.J<-th(envwav.1.1)
envwav.1.37<-env(wav.1.1,f=22050,plot=FALSE);result.th.K<-th(envwav.1.1)
envwav.1.38<-env(wav.1.1,f=22050,plot=FALSE);result.th.L<-th(envwav.1.1)
envwav.1.39<-env(wav.1.1,f=22050,plot=FALSE);result.th.M<-th(envwav.1.1)
envwav.1.40<-env(wav.1.1,f=22050,plot=FALSE);result.th.N<-th(envwav.1.1)
envwav.1.41<-env(wav.1.1,f=22050,plot=FALSE);result.th.O<-th(envwav.1.1)
envwav.1.42<-env(wav.1.1,f=22050,plot=FALSE);result.th.P<-th(envwav.1.1)
envwav.1.43<-env(wav.1.1,f=22050,plot=FALSE);result.th.Q<-th(envwav.1.1)
envwav.1.44<-env(wav.1.1,f=22050,plot=FALSE);result.th.R<-th(envwav.1.1)
envwav.1.45<-env(wav.1.1,f=22050,plot=FALSE);result.th.S<-th(envwav.1.1)
envwav.1.46<-env(wav.1.1,f=22050,plot=FALSE);result.th.T<-th(envwav.1.1)
envwav.1.47<-env(wav.1.1,f=22050,plot=FALSE);result.th.U<-th(envwav.1.1)
envwav.1.48<-env(wav.1.1,f=22050,plot=FALSE);result.th.V<-th(envwav.1.1)
envwav.1.49<-env(wav.1.1,f=22050,plot=FALSE);result.th.W<-th(envwav.1.1)
envwav.1.50<-env(wav.1.1,f=22050,plot=FALSE);result.th.X<-th(envwav.1.1)
envwav.1.51<-env(wav.1.1,f=22050,plot=FALSE);result.th.Y<-th(envwav.1.1)
envwav.1.52<-env(wav.1.1,f=22050,plot=FALSE);result.th.Z<-th(envwav.1.1)
envwav.1.53<-env(wav.1.1,f=22050,plot=FALSE);result.th.aa<-th(envwav.1.1)
envwav.1.54<-env(wav.1.1,f=22050,plot=FALSE);result.th.ab<-th(envwav.1.1)
envwav.1.55<-env(wav.1.1,f=22050,plot=FALSE);result.th.ac<-th(envwav.1.1)
envwav.1.56<-env(wav.1.1,f=22050,plot=FALSE);result.th.ad<-th(envwav.1.1)
envwav.1.57<-env(wav.1.1,f=22050,plot=FALSE);result.th.ae<-th(envwav.1.1)
envwav.1.58<-env(wav.1.1,f=22050,plot=FALSE);result.th.af<-th(envwav.1.1)
envwav.1.59<-env(wav.1.1,f=22050,plot=FALSE);result.th.ag<-th(envwav.1.1)
envwav.1.60<-env(wav.1.1,f=22050,plot=FALSE);result.th.ah<-th(envwav.1.1)

th.result<-rbind(result.th.a,result.th.b,result.th.c,
                  result.th.d,result.th.e,result.th.f,
                  result.th.g,result.th.h,result.th.i,
                  result.th.j,result.th.k,result.th.l,
                  result.th.m,result.th.n,result.th.o,
                  result.th.p,result.th.q,result.th.r,
                  result.th.s,result.th.t,result.th.u,
                  result.th.v,result.th.w,result.th.x,
                  result.th.y,result.th.z,result.th.A,
                  result.th.B,result.th.C,result.th.D,
                  result.th.E,result.th.F,result.th.G,
                  result.th.H,result.th.I,result.th.J,
                  result.th.K,result.th.L,result.th.M,
                  result.th.N,result.th.O,result.th.P,
                  result.th.Q,result.th.R,result.th.S,
                  result.th.T,result.th.U,result.th.V,
                  result.th.W,result.th.X,result.th.Y,
                  result.th.Z,result.th.aa,result.th.ab,
                  result.th.ac,result.th.ad,result.th.ae,
                  result.th.af,result.th.ag,result.th.ah)
View(th.result)

setwd("C:/Work")
resultEE<-read.csv("spectral properties EE.csv")
#View(resultEE)
attach(resultEE)
plot(kurtosis, type='l')
plot(sh, type='l')
plot(skewness, type='l')
plot(cent,type='l')
plot(ACI[4:60], type='l')
plot(zcr,type="l")
plot(sfm,type='l')
plot(sd,type='l')
plot(th,type='l')

for(i in 1:6) { #-- Create objects  'r.1', 'r.2', ... 'r.6' --
        nam <- paste("r", i, sep = ".")
        assign(nam, 1:i)
}
