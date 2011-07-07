/*	sobel_edge_detect.c
 *	Examples:
 *      edged_image = sobel_edge_detect(bw_im);		
 *		
 *  Notes:
 *      Takes a UINT8 image (Max of 255)
 *      This takes a Black and White image and produces an Edged image.
 *
 *  To Build:
 *      mex -v sobel_edge_detect.c
 *
 *	Author:
 *		Anthony Gabrielson
 *		agabriel@home.tzo.org
 *      12/7/2005
 *
 *	Modifications:
 *		05/11/2006: Fixed SUM = labs(sumX) + labs(sumX) bug.
 *		05/11/2006:	Images are now held in a struct; seems cleanier and no globals.
 */

#include "mex.h"
#include <string.h>             // Needed for memcpy() 

#define NDIMS           2       //X * Y

#define	SUCCESS			0
#define	MALLOCFAIL		-1
#define IMPROPERDIMS	-2

typedef unsigned char   uint8;

typedef struct image{
	uint8	*im;
	int     dims[NDIMS];
}image;

int	sobel_edge( image *bw_im, image *ed_im );
int getData( const mxArray **prhs, image *bw_im );
int sendData( mxArray **plhs, image *ed_im );

/*
 *  mexFunction:  Matlab entry function into this C code
 *  Inputs: 
 *      int nlhs:   Number of left hand arguments (output)
 *      mxArray *plhs[]:   The left hand arguments (output)
 *      int nrhs:   Number of right hand arguments (inputs)
 *      const mxArray *prhs[]:   The right hand arguments (inputs)
 *
 * Notes:
 *      (Left)  goes_out = foo(goes_in);    (Right)
 */
void mexFunction(int nlhs, mxArray *plhs[], int nrhs, const mxArray *prhs[])
{
	image	bw_im, ed_im;
	int		status;
  	//requires one variable, the image.
    if( nrhs > 0 ){
		if( (status = getData( &prhs[0], &bw_im )) != SUCCESS ){
			return;
		}
	} else {
        mexErrMsgTxt("No Input...\n");
	}

    //Run the edge detection algorithm
	if( (status = sobel_edge( &bw_im, &ed_im )) != SUCCESS ){
		if(status == MALLOCFAIL)
			free( bw_im.im );
		return;
	}
    
    //Send the edged image back to Matlab.
    sendData( &plhs[0], &ed_im);
    
    free( bw_im.im );
    free( ed_im.im );
    return;
}

/*
 *  sobel_edge:  Converts the BW image in an Edged image.
 *  Inputs: 
 *      (image *) Org Black and White image struct.
 *		(image *) The Edge Detected image struct.
 *
 *  Returns:
 *      int: 0 is successful run. -1 is a bad malloc.
 *
 *  How this algorithm works:
 *      http://www.pages.drexel.edu/~weg22/edge.html
 */
int sobel_edge( image *bw_im, image *ed_im )
{
    int     X, Y, I, J, elements, im_offset, mask_offset;
    long	sumX, sumY, SUM;
    int     x_mask[]={-1,-2,-1,
                       0, 0, 0,
                       1, 2, 1};
    int     y_mask[]={ 1, 0,-1,
                       2, 0,-2,
                       1, 0,-1};    
    
    elements = bw_im->dims[0] * bw_im->dims[1];
	ed_im->dims[0] = bw_im->dims[0];
	ed_im->dims[1] = bw_im->dims[1];
    
	if ( (ed_im->im = malloc(sizeof(uint8) * elements)) == NULL ){
        mexWarnMsgTxt("Edge im malloc failed...\n");
		return MALLOCFAIL;
	}
     // Convolution starts here
    for(Y=0; Y<ed_im->dims[1]; Y++)  {
        for(X=0; X<ed_im->dims[0]; X++)  {
            // image boundaries 
            if( Y==0 || Y==ed_im->dims[1]-1){ /*rows*/
                SUM = 0;
            } else if( X==0 || X==ed_im->dims[0]-1) { /*cols*/
                SUM = 0;
            // Convolution starts here
            } else {
                sumX = 0; sumY = 0; SUM = 0;
                // X&Y GRADIENT APPROXIMATION
                for(I=-1; I<=1; I++)  {
                    for(J=-1; J<=1; J++)  {
                        im_offset = (Y+J)*ed_im->dims[0] + (X+I);
                        mask_offset = (I+1)+((J+1)*3);
						sumX += (int)
                            (*(bw_im->im+im_offset)) * 
                            x_mask[mask_offset];
                        sumY += (int)
                            (*(bw_im->im+im_offset)) * 
                            y_mask[mask_offset];
                    }
                }
             }
             SUM = labs(sumX) + labs(sumY);
			 if(SUM > 255)	SUM = 255;
			 else if(SUM < 0)	SUM = 0;
            *(ed_im->im+Y*ed_im->dims[0]+X) = 255 - (uint8) SUM;  
	     
        }
    }
   
	return SUCCESS;
}

/*
 *  getData:  Gets data from a Matlab argument.
 *  Inputs: 
 *      const mxArray **prhs: Right hand side argument with RGB image
 *		(image *) Pointer to the black and white image struct.
 *
 *  Returns:
 *      int: 0 is successful run. -1 is a bad malloc. -2 is improper dims. 
 */
int getData( const mxArray **prhs, image *bw_im )
{ 
    uint8      *pr; 
    int         index, number_of_dimensions, total_elements; 
    const int   *ldims;
    
     if (mxIsNumeric(*prhs) == 0) 
		mexErrMsgTxt("Not numbers...\n");
    
    for (index=0; index<NDIMS; index++)
        bw_im->dims[index]=0;
    
    total_elements = mxGetNumberOfElements(*prhs);
    number_of_dimensions = mxGetNumberOfDimensions(*prhs);
    ldims = mxGetDimensions(*prhs);
    for (index=0; index<number_of_dimensions; index++)
        bw_im->dims[index] = ldims[index];
    
    pr = (uint8 *)mxGetData(*prhs);
    
	if( number_of_dimensions > NDIMS ){
        mexWarnMsgTxt("This input exceeds proper dimensions...\n");
		return IMPROPERDIMS;
	}
    
    //Allocated the space
	if ( (bw_im->im = malloc(sizeof(uint8) * total_elements)) == NULL ){
        mexWarnMsgTxt("im malloc failed...\n");
		return MALLOCFAIL;
	}

    //Get the image
	memcpy(bw_im->im, pr, sizeof(uint8) * total_elements);
    
    return SUCCESS;
}

/*
 *  sendData:  Sends data back to a Matlab argument.
 *  Inputs: 
 *      mxArray **plhs: Left hand side argument to get edge detected image
 *      (image *) Image to go back to Matlab.
 */
int sendData( mxArray **plhs, image *ed_im )
{
    uint8 *start_of_pr;   
    int bytes_to_copy, elements;

    elements = ed_im->dims[0] * ed_im->dims[1];
   
    // Create a dims[0] by dims[1] array of unsigned 8-bit integers. 
    *plhs = mxCreateNumericArray(NDIMS,ed_im->dims,mxUINT8_CLASS,mxREAL); 
                                  
    // Populate the the created array.
    start_of_pr = (uint8 *) mxGetData(*plhs);
    bytes_to_copy = ( elements ) * mxGetElementSize(*plhs);
    memcpy(start_of_pr, ed_im->im, bytes_to_copy);
  
    return SUCCESS;
} 

