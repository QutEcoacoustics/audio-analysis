# Creates a single image that shows ribbon plots stacked vertically
# Anthony Truskinger 2018
#
# [2019] Deprecated: see DrawRibbonPlot command
#
# For each image file with the phrase "SpectralRibbon" in it's name
#  found in a given directory it:
# - Groups the image based on it's type (e.g. ACI-ENT-EVN)
# - Sorts the images by date
# - Joins the spectral ribbons together to make a ribbon plot
# - Generates a date strip image
# - Then joins the date strip image to the ribbon plot
# - And saves the resulting image to the same folder
#
# Assumptions:
# - The input directories contain indices results
# - You want a grid of 1xN images (This could be paramterized in an update to the script)
# - The user will modify the $working_dirs variable before they run the script


$working_dirs = @(
    "Y:\Results\20181010-115050\ConcatResult\TimedGaps\Sturt\2015July\Mistletoe",
    "Y:\Results\20181010-115050\ConcatResult\TimedGaps\Sturt\2016Sep\Stud\CardA"
)

$image_wildcard = "*.SpectralRibbon.png"


foreach ($working_dir in $working_dirs) {
    Write-Output "Generating ribbon plots for $working_dir"
    $all_images = Get-ChildItem -Recurse $working_dir -Include $image_wildcard

    # sort and bucket images
    $image_types = @{}

    foreach ($image in $all_images) {
        if ($image.FullName -match ".*(\d{8}).*__([-\w]{11})\..*") {
            $date = [datetime]::ParseExact($Matches[1], "yyyyMMdd", [cultureinfo]::InvariantCulture)
            $type = $Matches[2]

            $image_date = [PSCustomObject]@{
                Date = $date
                File = $image
            }

            if (-not $image_types[$type]) {
                $image_types[$type] = @(, $image_date)
            }
            else {
                $image_types[$type] += $image_date
            }
        }
        else {
            throw "Unexpected file name pattern encountered $image"
        }
    }

    foreach ($image_type in $image_types.Keys) {

        $sorted_images = $image_types[$image_type] | Sort-Object Date

        $n = $sorted_images.Count
        # stack ribbons
        $command = "magick montage -background '#333' -tile '1x' -gravity West -geometry +4+4 "
        $command += ($sorted_images | ForEach-Object {
        
                " '$($_.File.FullName)' "
            }) -join ' '
        $command += " '$working_dir/stacked_ribbon_plot_$image_type`_ribbons.png'"

        Write-Output "Generating ribbon plot for $image_type..."
        Invoke-Expression $command
    
        # stack labels
        $command = "magick -background '#333' -fill '#FFF'  -gravity Center  "
        $command += ($sorted_images | ForEach-Object {
                $date = $_.Date.ToString("yyyy-MM-dd")
                " -size 200x40  -gravity center label:'$date'"
            }) -join ' '
        $command += " -append '$working_dir/stacked_ribbon_plot_$image_type`_dates.png'"

        Write-Output "Generating y-axis for $image_type..."
        Invoke-Expression $command

        # combine labels, ribbons, and then stick an axis on the bottom
        $two_map = (Get-ChildItem ($sorted_images[0].File.Directory.FullName + "/*2Maps.png")).FullName
        Write-Output "Joining ribbon plot, y-axis, and x-axis for $image_type..."
        magick "$working_dir/stacked_ribbon_plot_$image_type`_dates.png" "$working_dir/stacked_ribbon_plot_$image_type`_ribbons.png" +append "xwd:-" |   magick -gravity SouthEast "xwd:-" `(  "$two_map" -crop "1440x18+0+0" -background '#333' -splice 4x0+0+0 `) -append "$working_dir/stacked_ribbon_plot_$image_type.png"

    }
}