
#' @export
example.1 <- function () {
  
  output.suffix <- gsub(" ", "_", date());
  
  
  data <- data.frame(
    country = c("Australia", "Australia","Italy","Italy","Italy"),
    name = c("Brisbane","Sydney","Roma","Napoli","Rimini"),
    population = c(2000000, 4000000, 6000000, 3000000, 100000),
    img = c("Brisbane.jpg","Sydney.jpg","Roma.jpg","Napoli.jpg","Rimini.jpg")
  )
  
  
  HtmlInspector(template.path = 'inst/example_templates/cities.html', output.path =  paste0('inst/example_output/cities_',output.suffix,'.html'), data)  
  
  
}

example.2 <- function () {
  output.suffix <- gsub(" ", "_", date());
  template <- "/Users/eichinsp/Documents/github/audio-analysis/AudioAnalysis/RCode/Phil/sampleselection/source/templates/segment.event.inspector.html"
  output.path <-  paste0('inst/example_output/segments_',output.suffix,'.html')
  data <- read.csv("/Users/eichinsp/Documents/github/audio-analysis/AudioAnalysis/RCode/Phil/sampleselection/source/test.segments.for.output.csv")
  
  HtmlInspector(template.path = template, output.path =  output.path, data, "inspect segments")
  
}

example.3 <- function () {
  output.suffix <- gsub(" ", "_", date());
  template <- "/Users/eichinsp/Documents/github/audio-analysis/AudioAnalysis/RCode/Phil/sampleselection/source/templates/segment.event.inspector.html"
  output.path <-  paste0('inst/example_output/segments_',output.suffix,'.html')
  data <- read.csv("/Users/eichinsp/Documents/github/audio-analysis/AudioAnalysis/RCode/Phil/sampleselection/source/test.segments.for.output.3.csv")
  
  HtmlInspector(template.path = template, output.path =  output.path, data, "inspect segments")
  
}