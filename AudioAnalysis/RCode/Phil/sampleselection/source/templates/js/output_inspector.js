
oi = {};

oi.config = {
    total_height: 600, // svg height
    total_width: 1000, // svg width
    group_height: 60, // height of a row
    group_spacing: 20, // s
    group_title_height: 20,
    max_v_width: 30,
    popup_height: 200,
    popup_width: 200,
    grey: 0.2,
};

oi.init = function () {

    this.svg = d3.select("body")
        .append("svg")
        .attr("width", oi.config.total_width)
        .attr("height", oi.config.total_height)
        .classed('canvas',true);

    this.g_names = Object.keys(data);

    this.makeGroupMap();
    this.setupOrder();
    this.setNumRows();

    this.setupGroups();
    oi.draw();


    this.tooltip = d3.select('body').append('div')
        .style('position', 'absolute')
        .style('padding', '0 10px')
        .style('background', 'white')
        .style('opacity', 0)
        .classed('tooltip', true)

};

/**
 * Sets the row number of each group, ensuring that
 * the row number is greater than the row number of all the dependencies
 * so that links always travel down
 */
oi.setupOrder = function () {

    // first initialise the rows to 0
    data.forEach(function (g) {
        g.row = 0;
    });


    // for each group, loop through its versions
    // for each version, loop through its dependencies
    // add the row num of each dependency's group to the d_rows
    // make the current group one row more than the max row of its dependencies
    // keep doing this over and over until no more row numbers are changing

    var finished = false;
    while(!finished) {
        finished = true;
        data.forEach(function (g) {

            var d_rows = [], // the row of each of this group's version's dependencies
                d_rows_max; // the max of d_rows
            g.versions.forEach(function (v) {
                Object.keys(v.links).forEach(function(d){
                    d_rows.push(oi.getGroup(d).row);
                });
            });

            d_rows_max = (d_rows.length > 0) ? Math.max.apply(null, d_rows) : 0;
            if (d_rows_max + 1 > g.row) {
                g.row = d_rows_max + 1;
                finished = false;
            }
        });
    }

};


/**
 * sets the total number of rows based on the row numbers of each group
 */
oi.setNumRows = function () {
    oi.num_rows = 0;
    oi.g_names.forEach(function(g){
        oi.num_rows = Math.max(oi.num_rows, data[g].row);
    });
};


/**
 * create an object where name is group name and val is index of data
 * to be used as a shortcut to get group by
 */
oi.makeGroupMap = function () {
    oi.group_map = {};
    for (var g = 0; g < data.length; g++) {
        oi.group_map[data[g].name] = g;
    }
};

/**
 * Returns the element of data with the given name
 * @param string name
 * @returns {*}
 */
oi.getGroup = function (name) {
    return(data[oi.group_map[name]]);
};


/**
 * Draw each group on the svg
 */
oi.setupGroups = function () {

    var r, row_groups, row_y, g_x, v_width, num_v_in_row, next_x;

    // keep a list of all versions so we can iterate over all of them
    oi.versions = [];

    for (r = 1; r <= oi.num_rows; r++) {

        row_groups = data.filter(function (g) {
            return g.row == r;
        });

        row_y = (r * oi.config.total_height / oi.num_rows) - oi.config.group_height - oi.config.group_spacing;

        // the width of the version (member of group) should be consistent within a row
        num_v_in_row = 0;
        row_groups.forEach(function (g) {
            num_v_in_row += g.versions.length;
        });

        v_width = (oi.config.total_width - (oi.config.group_spacing * (row_groups.length + 1))) / num_v_in_row;
        v_width = Math.min(v_width, oi.config.max_v_width);

        row_groups.forEach(function (g, i) {

            var g_x;



                if (i == 0) {
                    g_x = oi.config.group_spacing;
                } else {
                    g_x = row_groups[i-1].pos.x + row_groups[i-1].pos.w + oi.config.group_spacing;
                }



            g.pos = {
                x:g_x,
                y:row_y,
                h:oi.config.group_height,
                w:g.versions.length * v_width
            };

            g.versions.forEach(function(v, j) {

                // unique string to identify this version
                v.id = g.name + v.v;
                v.pos = {
                    x:j * v_width,
                    h:oi.config.group_height - oi.config.group_title_height,
                    w:v_width
                };
                v.group_name = g.name;
                v.group = g; //!! circular!
                v.group_pos = g.pos;
                v.globalPos = function () {
                    return({
                        x: this.pos.x + this.group_pos.x,
                        y: this.group_pos.y + oi.config.group_title_height
                    })

                };
                v.state = 1;

                oi.versions.push(v);


                // up links and down links will create circular references. Hopefully won't cause problems!
                // a 'link' is an object with properties from, to, line.
                v.up_links = oi.getVs(v.links).map(function (up_v) {
                    return {
                        from: up_v,
                        to: v
                    };
                });
                v.down_links = []; // to be populated after all

            });

        });
    }

    // now that all versions have a down_list, populate it
    // circular as hell!!
    oi.versions.forEach(function(v) {
        v.up_links.forEach(function (up_link) {
            up_link.from.down_links.push(up_link);
        });
    });

    oi.optimisePositions();

};


/**

 * @constructor
 */
oi.optimisePositions1 = function () {

    var row_groups;

    for (r = 1; r <= oi.num_rows; r++) {

        row_groups = oi.getRow(r);

        // first put the row groups in their individual optimal position
        // which may make them overlap
        row_groups.forEach(function (g) {
            oi.setOptimalPosition([g], true, true);
        });



        // then order them by their center x
        row_groups.sort(function (a,b) {
            return (a.pos.x + (a.pos.w / 2) > b.pos.x + (b.pos.w / 2));
        });



        // then move them to the right until they are the minimum spacing
        for (var g_num = 1; g_num < row_groups.length; g_num ++ ) {
            min_x = row_groups[g_num - 1].pos.x + row_groups[g_num - 1].pos.w + oi.config.group_spacing;
            if (row_groups[g_num].pos.x < min_x) {
                row_groups[g_num].pos.x = min_x;
            }
        }


        // now, slide the whole row to the optimal while keeping it locked
        oi.setOptimalPosition(row_groups, true, true);

    }

};

oi.getRow = function (row_num) {
    return data.filter(function (g) {
        return g.row == row_num;
    });
}


/**
 * Tries to set the horizontal position of groups
 * to minimise the total link distance
 *
 * repeatedly goes through all groups in random order
 * first optimising for position, then correcting for overlap
 * it checks the position before and after to see if the group moved
 * if groups stop moving, terminate
 *
 */
oi.optimisePositions = function () {
    var order, g, pos_before;
    var still_moving = true,
        count = 0;
    while(still_moving && count < 100) {

        count++;
        //order = util.randomArray(data.length);
        data.forEach(function (g) {
            oi.setForces([g], true, true);
        });

        still_moving = oi.moveTowardsOptimal();

        oi.draw();

    }

    for (var r = 1; r < oi.num_rows; r++ ) {
        oi.fixOverlapping(r);
    }

};
/**
 * get the position for the group that minimises the total link offsets
 */
oi.setForces = function (groups, up_links, down_links) {

    var link_offsets, av_offset, total_offset;
    link_offsets = groups.map(function (g) {
        return g.versions.map(function (v) {
            var vos = [];
            if (up_links) {
                vos = vos.concat(v.up_links.map(function (link){
                    // TODO: minimise the line angles rather than the x offsets
                    return (link.from.globalPos().x - link.to.globalPos().x) / Math.abs(link.from.group.row - link.to.group.row);
                }));
            }
            if (down_links) {
                vos = vos.concat(v.down_links.map(function (link){
                    // TODO: minimise the line angles rather than the x offsets
                    return (link.to.globalPos().x - link.from.globalPos().x) / Math.abs(link.from.group.row - link.to.group.row);
                }));
            }
            return vos;
        });
    });
    // flatten 3d array to 1d
    link_offsets = [].concat.apply([],[].concat.apply([],link_offsets));
    // get sum and divide by length

    total_offset = link_offsets.reduce((prev, curr) => prev + curr);
    av_offset =  total_offset / link_offsets.length;

    groups.forEach(function (g) {
        g.pos.force = total_offset;
        g.pos.optimal_dist = av_offset;
    });

};
oi.moveTowardsOptimal = function () {
    var has_moved = false,
        change;
    data.forEach(function (g) {
        change = (g.pos.optimal_dist * 0.4)
        g.pos.x = g.pos.x + change;
        if (Math.abs(change) > 1) {
            // if this group moved more than 1 pixel in this update
            has_moved = true;
        }
    });
    return has_moved;
}

/**
 * for all pairs in the group
 * check if they are overlapping and if so move them away from
 * move both this group and the overlapping ones away from
 * each other until they are the min distance
 * since later fixes might re-overlap earlier fixes, repeat
 * until stable
 * @param g
 */
oi.fixOverlapping = function (row) {
    // TODO: make this work faster
    var moved,
    row_groups = oi.getRow(row),
    ok = false,
    count = 0,
    max_iterations = 50;
    while(!ok && count < max_iterations) {
        ok = true;
        count++;
        for (var i = 0; i < row_groups.length; i++) {
            for (var j = 0; j < i; j++) {
                moved = oi.fixOverlappingPair(row_groups[i], row_groups[j]);
                if (moved) {
                    ok = false;
                }
            }
        }
    }

    if (count == max_iterations) {
        console.log("row "+row+" not finished fixing overlap");
    }

};

/**
 * give a pair of groups, checks if they are overlapping
 * and if so moves them both apart until they are not
 * @param a
 * @param b
 * @returns true if something moved false if not
 */
oi.fixOverlappingPair = function (a,b) {
    var change_amount;
    var moved = oi.bringInside([a]) || oi.bringInside([b]);

    var ol_r = a.pos.x + a.pos.w + oi.config.group_spacing - b.pos.x;
    var ol_l = b.pos.x + b.pos.w + oi.config.group_spacing - a.pos.x;

    // nothing overlapping, return whether moving inside moved anything
    if (ol_r <= 0 || ol_l <= 0) {
        return moved;
    }

    // figure out which way to resolve overlapping
    if (ol_r > ol_l) {
        change_amount = 0.5 * ol_l;
    } else {
        change_amount = -0.5 * ol_r;
    }

    a.pos.x += change_amount;
    b.pos.x -= change_amount;

    moved = Math.abs(change_amount) > 2

    if (!moved) {
        return false;
    }

    oi.bringInside([a,b]);

    return true;




};


/**
 * moves the groups by the same amount
 * so that none of them are outside the canvas
 * (unless they don't fit)
 * @param gs
 */
oi.bringInside = function (gs) {


    left_over = Math.min.apply(null, [0].concat(gs.map(function (g) {
        return(g.pos.x);
    })));
    right_over = Math.max.apply(null, [0].concat(gs.map(function (g) {
        return(g.pos.x + g.pos.w - oi.config.total_width);
    })));

    gs.forEach(function (g) {
        g.pos.x -= left_over;
        g.pos.x -= right_over;
    });

    return (left_over !== 0 || right_over !== 0);


};





oi.draw = function () {
    oi.svg.selectAll("g").remove();
    oi.svg.selectAll("rect").remove();
    oi.svg.selectAll("line").remove();
    for (var r = 1; r <= oi.num_rows; r++) {
        data.filter(function (g) {
            return g.row == r;
        }).forEach(function (g) {
            oi.drawGroup(g);
            g.versions.forEach(function(v) {
                oi.drawV(v, g.el)
            });
        });
    }
    oi.drawLinks();


}

/**
 * draws the svg elements for a group
 * @param obj
 */
oi.drawGroup = function (obj) {
    var res;
    obj.el = oi.svg.append('g');
    obj.el.append('rect')
        .attr("class", 'group')
        .attr("height", obj.pos.h)
        .attr("width", obj.pos.w)
        .attr("rx", 5)
        .attr("ry", 5)
        .attr('x',0)
        .attr('y',0);
    text = obj.el.append('text')
        .text(obj.name)
        .attr('y', oi.config.group_title_height / 2)
        .attr('x',obj.pos.w / 2)
    util.scaleTextToFit(text.node(), obj.pos.w - 4, oi.config.group_title_height);

    oi.setPos(obj);

};

/**
 * Draws the svg elements and sets up the mouse events
 * for versions
 * @param obj: Object; the version object
 * @param parent: svg g element of the group
 */
oi.drawV = function (obj, parent) {
    var res = {};
    res.rect = parent.append('rect')
        .attr("class", 'version')
        .attr("x", obj.pos.x)
        .attr("y", oi.config.group_title_height)
        .attr("height", obj.pos.h)
        .attr("width", obj.pos.w);
    res.text = parent.append('text')
        .text(obj.v)
        .attr("x", obj.pos.x + obj.pos.w / 2)
        .attr("y", oi.config.group_title_height + 20)
        .attr('fill', '#990000')
        .attr('class', 'v_num');
    res.rect.on("mouseover", function (v) {
            return (function () {
                oi.vMouseOver(v);
            });
        }(obj))
        .on("mouseout", oi.vMouseOut)
        .on("click", function (v) {
            return (function () {
                oi.vClick(v);
            });
        }(obj));
    obj.el = res;
};

/**
 *
 * @param g
 */
oi.setPos = function (g) {
    g.el.attr("transform", 'translate('+ g.pos.x+','+ g.pos.y+')')
};

/**
 * Actions that happen when the version rect is rolled over
 * - sets opacity, positionand html of tooltip
 * @param v: Object; the version object
 */
oi.vMouseOver = function (v) {

    oi.tooltip.transition()
        .style('opacity', .9)

    oi.tooltip.html(oi.tooltipText(v))
        .style('left', (v.globalPos().x) + 'px')
        .style('top',  (v.globalPos().y + v.pos.h + 20) + 'px')

    oi.activate(v, 'over', true);

};

/**
 * Actions that happen when a version rect is rolled off
 * - hides tooltip
 */
oi.vMouseOut = function () {
    oi.tooltip.transition()
        .style('opacity', 0)
    oi.removeClassFromAll('over');
};



/**
 * Actions that happen when the version rect is clicked.
 * - cycles through the "state" of the version:
 *   - 1: not active
 *   - 2: activate it and its dependencies
 *   - 3: activate it, its dependencies and its dependents
 * @param v: Object; the version object
 */
oi.vClick = function (v) {
    new_state = (v.state == 3) ? 1 : v.state + 1;

    // no state 2 for v with no up links
    if (v.up_links.length == 0 && new_state == 2) {
        new_state = 3;
    }


    // set all version state to 1
    oi.versions.forEach(function (cur_v) {
        cur_v.state = 1;
    });

    oi.removeClassFromAll('active');


    // set cur version state to v.state
    v.state = new_state;

    // if v_state > 1, recursive to set active state to this and dependencies
    // if v_state > 2, recusive to set active state to all dependants
    if (new_state > 1) {
        oi.activate(v, 'active', (new_state > 2));
    }

    v.el.rect.classed('main');


};

oi.removeClassFromAll = function (class_name) {
    oi.svg.selectAll('rect').classed(class_name, false);
    oi.svg.selectAll('line').classed(class_name, false);
};

oi.activate = function (v, class_name, activate_down) {
    // keep track of which have been activated, so we don't get in a loop

    v.el.rect.classed(class_name, true);

    // list of versions and lines to activate
    var active_links = [];

    oi.getChain(v, true, active_links);

    if (activate_down) {
        oi.getChain(v, false, active_links);
    }

    active_links.forEach(function (l) {
        l.line.classed(class_name, true);
        l.from.el.rect.classed(class_name, true);
        l.to.el.rect.classed(class_name, true);
    });

};

oi.getChain = function (v, up, active_links) {



    var links;

    links = (up) ? v.up_links : v.down_links;

    // check for loops
    links.forEach(function (l){
        if (active_links.indexOf(links[l]) == -1) {
            active_links.push(l);
            oi.getChain((up ? l.from : l.to), up, active_links);
        }
    });



    return links;


};




oi.tooltipText = function (v) {

    template =  "<p> version: [v] </p>" +
        "<p> date: [d] </p>";



    map = {
        "[d]": v.date,
        "[v]": v.v
    }

    return(template.replace(/\[[a-z]\]/gi, function(matched){
        return map[matched];
    }));



};



/**
 * returns a version based on the name and version number
 */
oi.getV = function (name, version_num) {
    var v, g =  oi.getGroup(name);
    v = g.versions.filter(function (version) {
        return version.v == version_num;
    });
    return v[0];
};

/**
 * given a list of groupnames and versions, returns an array of version objects
 * @param v_list; object with properties like group_name: version
 */
oi.getVs = function (v_list) {
    res = [];
    Object.keys(v_list).forEach(function (group_name) {
        res.push(oi.getV(group_name, v_list[group_name]));
    })
    return(res);
};


/**
 * for each group, goes through each version
 * for each version, goes through the up links
 * and draws a line connecting the mid point of the bottom of the from-version
 * to the mid point of the top of the to-version
 */
oi.drawLinks = function () {

    data.forEach(function (g) {
        g.versions.forEach(function (v) {
            v.up_links.forEach(function(link) {

                var start_x, end_x, start_y, end_y,    l;

                d_global_pos = link.from.globalPos();
                v_global_pos = link.to.globalPos();

                start_x = d_global_pos.x + (link.from.pos.w / 2);
                end_x = v_global_pos.x + (link.to.pos.w / 2);
                start_y = d_global_pos.y + link.from.pos.h;
                end_y = v_global_pos.y;

                link.line = oi.svg.append('line')
                    .attr('x1', start_x)
                    .attr('y1', start_y)
                    .attr('x2', end_x)
                    .attr('y2', end_y)
                    .attr('stroke-width', 1)
                    .attr('stroke', 'black');

            });
        });
    });

};


util = {};
util.randomArray = function (size) {

    var a = [];
    for (var i = 0; i < size; i++) {
        a.push(i);
    }

        var j, x, i;
        for (i = a.length; i; i -= 1) {
            j = Math.floor(Math.random() * i);
            x = a[i - 1];
            a[i - 1] = a[j];
            a[j] = x;
        }

    return a;
};
util.scaleTextToFit = function (text_node, width, height) {

    var bb = text_node.getBBox();
    var widthTransform = width / bb.width;
    var heightTransform = height / bb.height;
    var value = widthTransform < heightTransform ? widthTransform : heightTransform;
    var right = (bb.width - bb.width * value) / 2;
    var down = (bb.height - bb.height * value) / 2
    text_node.setAttribute("transform", "scale("+value+") translate("+ right +","+down+")");

};


      
oi.init();
      

      
