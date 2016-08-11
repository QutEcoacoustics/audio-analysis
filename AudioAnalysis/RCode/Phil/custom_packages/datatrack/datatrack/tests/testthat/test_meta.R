context("Datatrack Meta")

correct.meta.cols <- c('name','version','params','dependencies','date','file.exists','col.names','callstack','csv','annotations','system')

test_that("Create Empty Meta returns a data frame with the correct columns and zero rows", {
    expect_is(.CreateEmptyMeta(), 'data.frame')
    expect_equal(nrow(.CreateEmptyMeta()), 0)
    expect_equal(colnames(.CreateEmptyMeta()),correct.meta.cols)
})

test_that("Fix Meta will return the a metadata file with the correct columns in the correct order when given a broken metadata file", {
    
    broken.meta <- data.frame(version = c(1,2,3,4), name = c('a', 'b', 'b', 'a'), badcol = 4:1)
    
    fixed.meta <- FixMeta(broken.meta)
    
    expect_is(fixed.meta, 'data.frame')
    expect_equal(colnames(fixed.meta), correct.meta.cols)

})


