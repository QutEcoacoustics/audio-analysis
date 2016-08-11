context("Datatrack Meta")

test_that("Create Empty Meta returns a data frame with the correct columns and zero rows", {
    expect_is(.CreateEmptyMeta(), 'data.frame')
    expect_equal(nrow(.CreateEmptyMeta()), 0)
    expect_equal(colnames(.CreateEmptyMeta()),
                 c('name','version','params','dependencies','date','file.exists','col.names','callstack','csv','annotations','system'))
})

