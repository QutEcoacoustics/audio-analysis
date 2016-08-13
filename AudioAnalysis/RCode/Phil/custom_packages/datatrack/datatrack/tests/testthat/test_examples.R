context("Datatrack Examples")


# This is a very broad test that executes all the examples. 
# doesn't guarantee that nothing is wrong, but at least will pick up 
# errors that stop being able to read or write data. 
# Note: there is no guarantee that the metadata resulting in the comparsion checksum
# is correct: there may be a bug that was unnoticed at the time that checksum was created. 
test_that("Examples run and produce correct metadata checksum", {
    RunAllExamples()
    expect_equal(GetChecksum(), "fd99dd08ab4fcda1520902f1a1c8873f9c0ca39b")
})
