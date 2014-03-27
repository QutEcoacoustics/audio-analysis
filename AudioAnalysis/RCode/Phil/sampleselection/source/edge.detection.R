library('adimpro')

source('ss.R')

f <- "/Users/n8933464/Documents/SERF/NW/test/test.wav"
i <- "/Users/n8933464/Documents/SERF/NW/test/test.wav.png"

spectro <- Sp.Create(f)

Sp.Draw(spectro, img.path = i)

ad <- make.image(spectro$vals)

edg <- edges(ad, type = "Laplacian", ltype=2, abs=FALSE)

edg.img <- extract.image(edg)

image(edg.img)