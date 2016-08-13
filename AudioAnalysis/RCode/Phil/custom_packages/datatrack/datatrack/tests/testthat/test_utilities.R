context("Datatrack Utilities")

test_that("MergeLists returns the correct list", {
    expect_equal(.MergeLists(
        list(a = 1, b = 'two', c = 3),
        list(b = 'three', d = 4)
    ), list(a = 1, b = 'two', c = 3, d = 4))
})

test_that("DateTime returns a string of length > 1", {
    expect_is(.DateTime(), 'character')
    expect_match(.DateTime(), '.')
})

test_that("AllSame returns true for a vector identical integers and false when they are not identical", {
    expect_true(.AllSame(c(10,10,10)))
    expect_false(.AllSame(c(10,10,10.1)))
})

test_that("toCsvValue converts objects to the right format", {

#TODO



})
