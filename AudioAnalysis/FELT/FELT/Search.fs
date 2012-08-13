module Search


    (*
        New module
        Objective: search for events

        Three stages:
        1) template construction
        2) point of interest detection
        3) POI classification (as a template)


        1) make use of work already done


        2) based on AED... in principal easy enough

        3) is where the real work will needs to begin
            - load templates

            - load aed events


                    
                - for each aed event
                    - get the nosie profile for the minute surrounding the event
                    - get the snippet of audio for those bounds. 
                        - add padding?
                        - run noise removal?

                    - use freq bounds on template to apply a bandpass
                        - padding?
                        - roll off response?


















    *)