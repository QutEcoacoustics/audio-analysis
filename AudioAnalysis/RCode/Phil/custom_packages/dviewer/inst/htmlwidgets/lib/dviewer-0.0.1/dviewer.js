
oi = {};

oi.config = {
    group_height: 60, // max height of a row
    group_spacing: 20, // s
    row_spacing: 0.2, // %
    group_title_height: 0.3,
    max_v_width: 30,
    popup_height: 200,
    popup_width: 200,
    grey: 0.2
};

oi.init = function (id, data, selected_group) {


    oi.container = d3.select("#"+id);

    oi.config.total_width = oi.container.node().getBoundingClientRect().width;
    oi.config.total_height = oi.container.node().getBoundingClientRect().height;

    oi.svg = oi.container
        .append("svg")
        .attr("width", oi.config.total_width)
        .attr("height", oi.config.total_height)
        .classed('canvas',true);

    oi.wrapper = oi.svg.append("g")
        .classed('wrapper',true);


    // oi.groups is the currently visible groups
    oi.all_groups = data;
    oi.makeGroupMap();
    oi.setupGroups();
    oi.selectGroup(selected_group);
    oi.tooltipInit();
    d3.select(window).on('resize', oi.resize);
    ff.init();

};



oi.selectGroup = function (group_name, cutoff_date) {
    oi.setHidden(group_name, cutoff_date);
    oi.setupOrder();
    oi.setNumRows();
    oi.resize();
};


oi.tooltipInit = function () {

    oi.curover = '';

    oi.tooltip = oi.container.append('div')
        .style('position', 'absolute')
        .style('opacity', 0)
        .classed('tooltip', true)
        .on('mouseover', oi.tooltipOver)
        .on('mouseout', oi.tooltipOut)

}


oi.resize = function () {

    oi.config.total_width = oi.container.node().getBoundingClientRect().width;
    oi.config.total_height = oi.container.node().getBoundingClientRect().height;
    oi.setupRows();
    oi.optimisePositions();
    oi.draw();
    oi.move();

};

/**
 * Sets the row number of each group, ensuring that
 * the row number is greater than the row number of all the dependencies
 * so that links always travel down
 */
oi.setupOrder = function () {

    // first initialise the rows to 0
    oi.groups.forEach(function (g) {
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
        oi.groups.forEach(function (g) {

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
    oi.num_rows = Math.max.apply(null, oi.groups.map(function (g) {
        return g.row;
    }));
};


/**
 * create an object where name is group name and val is index of data
 * to be used as a shortcut to get group by
 */
oi.makeGroupMap = function () {
    oi.group_map = {};
    for (var g = 0; g < oi.all_groups.length; g++) {
        oi.group_map[oi.all_groups[g].name] = g;
    }
};

/**
 * Returns the element of data with the given name
 * @param name string
 * @returns {*}
 */
oi.getGroup = function (name) {
    return(oi.all_groups[oi.group_map[name]]);
};


/**
 * adds extra properties to the data to make everything work
 */
oi.setupGroups = function () {

    oi.all_groups.forEach(function (g) {

        g.hidden = false;

        // make a copy of the versions array
        // all_versions will not change, while versions will hold the currently
        // visible versions. Makes it simpler to iterate over visible versions later
        g.all_versions = g.versions.slice(0);

        g.cur_pos = null;
        g.pos = {};

        g.all_versions.forEach(function(v, j) {

            // unique string to identify this version
            v.id = g.name + v.v;
            v.group = g; //!! circular!
            v.state = 1;

            // up links and down links will create circular references. Hopefully won't cause problems!
            // a 'link' is an object with properties from, to, line.
            v.all_up_links = oi.getVs(v.links).map(function (up_v) {
                return {
                    from: up_v,
                    to: v
                };
            });
            v.all_down_links = []; // to be populated after all
            v.cur_pos = null;
            v.pos = {};

        });

    });

    // now that all versions have a list of up links,
    // populate the corresponding versions' down links
    // circular as hell!!
    oi.all_groups.forEach(function (g) {
        g.all_versions.forEach(function(v) {
            v.all_up_links.forEach(function (up_link) {
                up_link.from.all_down_links.push(up_link);
            });
        });

    });

    oi.pos = {};
    oi.cur_pos = null;

};


/**
 * Sets values related to the rows
 * - width of versions in order to fit in rows
 * - y value of each row
 */
oi.setupRows = function () {

    var r, row_groups, row_y, v_width, num_v_in_row;

    // position values that will be the same for all groups/versions
    oi.pos = {};
    // total row  height including padding
    oi.pos.row_height = oi.config.total_height / oi.num_rows;

    // group height is the row height minus the padding OR the max row height, whichever is smaller
    oi.pos.group_height = Math.min(oi.config.group_height, oi.pos.row_height * (1-oi.config.row_spacing));
    oi.pos.group_title_height = oi.pos.group_height * oi.config.group_title_height;
    oi.pos.version_height = oi.pos.group_height * (1-oi.config.group_title_height);

    for (r = 1; r <= oi.num_rows; r++) {

        row_groups = oi.getRow(r);

        row_y = (r - 1) * oi.pos.row_height + (oi.config.row_spacing / 2);

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
                h:oi.pos.group_height,
                w:g.versions.length * v_width
            };

            g.versions.forEach(function(v, j) {

                v.pos = {
                    x:j * v_width,
                    w:v_width
                };

                v.globalPos = function (use_cur) {
                    if (use_cur) {
                        return({
                            x: this.cur_pos.x + this.group.cur_pos.x,
                            y: this.group.cur_pos.y + oi.cur_pos.group_title_height
                        })
                    } else {
                        return({
                            x: this.pos.x + this.group.pos.x,
                            y: this.group.pos.y + oi.pos.group_title_height
                        })
                    }

                };
            });
        });
    }
};

/**
 * Removes groups that are not dependencies or dependents of the given group
 * @param group_name
 */
oi.setHidden = function (group_name, cutoff_date) {

    // list of versions and lines to activate
    var selected_group, chain, combined_chain = [];

    selected_group = oi.getGroup(group_name);

    if (typeof(selected_group) === 'object') {

        // initialise hidden to true for all groups and versions
        oi.all_groups.forEach(function (g) {
            g.hidden = true;
            g.row = -1;
            g.selected = false;
            g.all_versions.forEach(function (v) {
                v.hidden = true;
            });
        });

        // get a chain of links that come from the given group
        oi.getGroup(group_name).all_versions.forEach( function (v) {
            chain = [];
            if (v.date >= cutoff_date) {
                oi.getChain(v, true, chain, true);
                oi.getChain(v, false, chain, true);
            }
            combined_chain = combined_chain.concat(chain);
        });


        // set the groups and versions that connect to each link in that chain to not hidden
        combined_chain.forEach(function (l) {
            l.from.group.hidden = false;
            l.to.group.hidden = false;
            l.from.hidden = false;
            l.to.hidden = false;
        });

        selected_group.selected = true;



    } else {

        // set hidden to false for all groups and versions, since group_name was not supplied
        oi.all_groups.forEach(function (g) {
            g.hidden = false;
            g.row = -1;
            g.selected = false;
            g.all_versions.forEach(function (v) {
                v.hidden = false;
            });
        });

    }



    // set versions to be the non hidden values from all_versions
    // set up_links and down_links to be the non_hidden values of all_up_links and all_down_links
    oi.all_groups.forEach(function (g) {
        g.all_versions.forEach(function (v) {
            v.up_links = v.all_up_links.filter(function(l) {
                return (!(l.from.hidden || l.to.hidden));
            });
            v.down_links = v.all_down_links.filter(function(l) {
                return (!(l.from.hidden || l.to.hidden));
            });
        });
        g.versions = g.all_versions.filter(function (v) {
            return !v.hidden;
        });
    });

    // copy all_groups that are not hidden to a new array
    oi.groups = oi.all_groups.filter(function (g) {
        return (!g.hidden);
    });

};


/**
 * returns an array containing the non-hidden groups from the specified row
 * @param row_num
 * @returns {Array|*|string[]|T[]}
 */
oi.getRow = function (row_num) {
    return oi.groups.filter(function (g) {
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
        //order = util.randomArray(oi.groups.length);
        oi.groups.forEach(function (g) {
            oi.setForces([g], true, true);
        });
        still_moving = oi.moveTowardsOptimal();
    }

    for (var r = 1; r < oi.num_rows; r++ ) {
        oi.fixOverlapping(r);
    }

    oi.center();

    oi.setInitialPositions();

};


/**
 * For any groups / versions that don't have a cur_pos set
 * (because they have just been added), set the cur_pos to the same as the final pos,
 * so that they are drawn in place.
 */
oi.setInitialPositions = function () {
    oi.groups.forEach(function (g) {
        if (g.cur_pos === null) {
            g.cur_pos = util.shallowClone(g.pos);
        }
        g.versions.forEach(function (v) {
            if (v.cur_pos === null) {
                v.cur_pos = util.shallowClone(v.pos);
            }
        });
    });
    if (oi.cur_pos === null) {
        oi.cur_pos = util.shallowClone(oi.pos);
    }
};


oi.center = function () {
    var min_left, max_right, shift;
    min_left = Math.min.apply(null, oi.groups.map(function (g) {
        return g.pos.x;
    }));

    max_right = Math.max.apply(null, oi.groups.map(function (g) {
        return g.pos.x + g.pos.w;
    }));

    shift = ((oi.config.total_width - max_right) - min_left) / 2;

    oi.groups.forEach(function (g) {
        g.pos.x += shift;
    });
};

/**
 * get the position for the groups that minimises the total link offsets
 * groups is an array of groups. If contains more than one group, will set the
 * forces as if they are fixed relative to each other
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

    if (link_offsets.length > 0) {
        total_offset = link_offsets.reduce(function (prev, curr) { return prev + curr;});
        av_offset =  total_offset / link_offsets.length;
    } else {
        // if there are no links at all, then just assume they are already where they need to be
        total_offset = 0;
        av_offset = 0;
    }


    groups.forEach(function (g) {
        g.pos.force = total_offset;
        g.pos.optimal_dist = av_offset;
    });

};

/**
 * Performs one step of moving towards the optimal position for all groups
 * @returns {boolean}
 */
oi.moveTowardsOptimal = function () {
    var has_moved = false,
        change;
    oi.groups.forEach(function (g) {
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

/**
 * Set the width, height, x and y of the svg elements
 */
oi.move = function () {

    // TODO:
    // 1) fade to remove newly hidden groups, versions and links
    // 2) expand existing groups to their new size (but don't shrink any that need to shrink)
    // 3) move

    // duration
    var d = 500;

    oi.config.total_width = oi.container.node().getBoundingClientRect().width;
    oi.config.total_height = oi.container.node().getBoundingClientRect().height;

    oi.svg
        .attr("width", oi.config.total_width)
        .attr("height", oi.config.total_height);

    oi.groups.forEach(function (g) {

        g.el.select('rect').transition()
            .attr("height", oi.pos.group_height)
            .attr("width", g.pos.w).duration(d)
        g.el.select('text').transition()
            .attr('y', oi.pos.group_title_height / 2 + 5)
            .attr('x',g.pos.w / 2).duration(d);
        oi.setPos(g.el, g.pos.x, g.pos.y, d);
        util.scaleTextToFit(g.el.select('text').node(), g.pos.w - 4, oi.pos.group_title_height);

        g.versions.forEach(function(v) {
            oi.setPos(v.el, v.pos.x, oi.pos.group_title_height, d);
            v.el.select('rect')
                .attr("height", oi.pos.version_height)
                .attr("width", v.pos.w)
            v.el.select('text')
                .attr("x",  v.pos.w / 2)
                .attr("y", 20);
        });

        if (g.selected) {
            g.el.classed('selected', true);
        } else {
            g.el.classed('selected', false);
        }

    });

    oi.moveLinks(d);

    oi.groups.forEach(function (g) {
        g.cur_pos = util.shallowClone(g.pos);
        g.versions.forEach(function (v){
            v.cur_pos = util.shallowClone(v.pos);
        });
    });
    oi.cur_pos = util.shallowClone(oi.pos);



};






/**
 * Draw all the svg elements
 */
oi.draw = function () {



    oi.all_groups.forEach(function (g) {
        if (g.hidden && typeof(g.el) === 'object') {
            // group is hidden but drawn, remove it
            g.el.remove();
            delete g.el;
            g.cur_pos = null;
        } else if (!g.hidden && typeof(g.el) !== 'object') {
            // group is not hidden and not drawn, so draw it
            oi.drawGroup(g);
        }
        g.all_versions.forEach(function(v) {
            if (v.hidden && typeof(v.el) === 'object') {
                // version is hidden but drawn, remove it
                v.el.remove();
                delete v.el;
                v.cur_pos = null;
            } else if (!v.hidden && typeof(v.el) !== 'object') {
                // version is not hidden and not drawn, so draw it
                oi.drawV(v, g.el)
            }
        });
    });

    oi.drawLinks();

    oi.zoom_b = d3.behavior.zoom()
        .center([oi.config.total_width / 2, oi.config.total_height / 2])
        .scaleExtent([1, 10])
        .on("zoom", function () {
            oi.wrapper.attr("transform", "translate(" + d3.event.translate + ")scale(" + d3.event.scale + ")");
        });

    oi.svg.call(oi.zoom_b);

};





/**
 * draws the svg elements for a group
 * @param obj
 */
oi.drawGroup = function (obj) {
    obj.el = oi.wrapper.append('g')
        .attr("class", 'group');
    obj.el.append('rect')
        .attr("rx", 5)
        .attr("ry", 5);
    text = obj.el.append('text')
        .text(obj.name);
    oi.setPos(obj.el, obj.cur_pos.x, obj.cur_pos.y, 0);
};

/**
 * Draws the svg elements and sets up the mouse events
 * for versions. Sets position based on the cur_pos of the object
 * @param obj: Object; the version object
 * @param parent: svg g element of the group
 */
oi.drawV = function (obj, parent) {
    obj.el = parent.append('g')
        .attr("class", 'version');
    obj.el.append('rect')
        .attr("width", obj.cur_pos.w)
        .attr("height", oi.cur_pos.version_height);
    obj.el.append('text')
        .text(obj.v)
        .attr('class', 'v_num')
        .attr("x",  obj.cur_pos.w / 2)
        .attr("y", 20);
    obj.el.on("mouseover", function (v) {
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
    oi.setPos(obj.el, obj.cur_pos.x, oi.cur_pos.group_title_height, 0);
};


/**
 * for each group, goes through each version
 * for each version, goes through the up links
 * and draws a line connecting the mid point of the bottom of the from-version
 * to the mid point of the top of the to-version
 */
oi.drawLinks = function () {

    oi.all_groups.forEach(function (g) {
        g.all_versions.forEach(function (v) {
            v.all_up_links.forEach(function(link) {

                if ((v.hidden || link.from.hidden) && typeof(link.line) === 'object') {
                    // link is from or to a hidden version, and is drawn, so remove it
                    link.line.remove();
                    delete link.line;
                } else if (!v.hidden && !link.from.hidden && typeof(link.line) !== 'object') {
                    // link is to and from non-hidden versions, but is not drawn, so draw it
                    link.line = oi.wrapper.append('line')
                        .attr('stroke-width', 1)
                        .attr('stroke', 'black');
                }
            });
        });
    });
    oi.moveLinks(0, true);

};

/**
 * sets the startx,y and endx,y for all links
 */
oi.moveLinks = function (d, use_cur) {

    oi.groups.forEach(function (g) {
        g.versions.forEach(function (v) {
            v.up_links.forEach(function(link) {



                var start_x, end_x, start_y, end_y, d_global_pos, v_global_pos;
                d_global_pos = link.from.globalPos(use_cur);
                v_global_pos = link.to.globalPos(use_cur);
                if (use_cur) {
                    from_pos = link.from.cur_pos;
                    to_pos = link.to.cur_pos;
                } else {
                    from_pos = link.from.pos;
                    to_pos = link.to.pos;
                }
                start_x = d_global_pos.x + (from_pos.w / 2);
                end_x = v_global_pos.x + (to_pos.w / 2);
                start_y = d_global_pos.y + oi.pos.version_height;
                end_y = v_global_pos.y;
                if (d > 0) {
                    link.line.transition()
                        .attr('x1', start_x)
                        .attr('y1', start_y)
                        .attr('x2', end_x)
                        .attr('y2', end_y).duration(d);
                } else {
                    link.line
                        .attr('x1', start_x)
                        .attr('y1', start_y)
                        .attr('x2', end_x)
                        .attr('y2', end_y);
                }

            });
        });
    });

};

/**
 *
 * @param g
 */
oi.setPos = function (el, x, y, d) {
    if (d > 0) {
        el.transition().attr("transform", 'translate('+ x+','+ y+')').duration(d);
    } else {
        el.attr("transform", 'translate('+ x+','+ y+')');
    }

};

/**
 * Actions that happen when the version rect is rolled over
 * - sets opacity, positionand html of tooltip
 * @param v: Object; the version object
 */
oi.vMouseOver = function (v) {

    var coords;

    clearTimeout(oi.tooltip_off);

    if (oi.curover === v.id) {
        // if we are already showing the relavent tooltip, do nothing
        return
    }

    oi.curover = v.id;

    oi.tooltip.transition()
        .style('opacity', .9)

    coords = d3.mouse(oi.svg.node());
    // offset so doesn't go outside container
    var x = Math.min(coords[0] + 20, oi.container.node().getBoundingClientRect().width - oi.tooltip.node().getBoundingClientRect().width - 20);

    var y = coords[1] + 20;

    y = (y + oi.tooltip.node().getBoundingClientRect().height > oi.container.node().getBoundingClientRect().height) ? coords[1] - 80 : y;

    oi.tooltip.html(oi.tooltipText(v))
        .style('left', x + 'px')
        .style('top',  y + 'px')



    oi.activate(v, 'over', true);

};

/**
 * Actions that happen when a version rect is rolled off
 * - hides tooltip
 */
oi.vMouseOut = function () {

    oi.hideTooltip();
};

oi.tooltipOver = function () {
    clearTimeout(oi.tooltip_off);
};

oi.tooltipOut = function () {
    oi.hideTooltip();
};

oi.hideTooltip = function () {
    oi.tooltip_off = setTimeout(function () {
        oi.classAll('over', false);
        oi.classAll('off', false);
        oi.tooltip.transition()
            .style('opacity', 0)
            .each("end", function () {
                oi.tooltip.style('left', -1000 + 'px')
                oi.tooltip.style('top',  -1000 + 'px')
            });
        oi.curover = '';
    }, 1000);
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
    oi.groups.forEach(function(g) {
        g.versions.forEach(function (cur_v) {
            cur_v.state = 1;
        });
    });

    oi.classAll('active', false);


    // set cur version state to v.state
    v.state = new_state;

    // if v_state > 1, recursive to set active state to this and dependencies
    // if v_state > 2, recusive to set active state to all dependants
    if (new_state > 1) {
        oi.activate(v, 'active', (new_state > 2));
    }

    v.el.classed('main');


};

oi.classAll = function (class_name, on) {
    oi.svg.selectAll('.version').classed(class_name, on);
    oi.svg.selectAll('.group').classed(class_name, on);
    oi.svg.selectAll('line').classed(class_name, on);
};

oi.activate = function (v, class_name, activate_down) {
    // keep track of which have been activated, so we don't get in a loop

    // set all to 'off'
    oi.classAll('over', false);
    oi.classAll('off', false);

    // list of versions and lines to activate
    var active_links = [];

    oi.getChain(v, true, active_links, false);

    if (activate_down) {
        oi.getChain(v, false, active_links, false);
    }

    v.el.classed(class_name, true);
    v.el.classed('off', false);
    v.group.el.classed(class_name, true);
    v.group.el.classed('off', false);

    active_links.forEach(function (l) {

        // remove the 'off' class from these and add the given class
        l.line.classed(class_name, true);
        l.from.el.classed(class_name, true);
        l.to.el.classed(class_name, true);
        l.to.group.el.classed(class_name, true);
        l.from.group.el.classed(class_name, true);

        l.line.classed('off', false);
        l.from.el.classed('off', false);
        l.to.el.classed('off', false);
        l.to.group.el.classed('off', false);
        l.from.group.el.classed('off', false);

    });

};

/**
 * recursive function that adds the links of the
 * version v to the link_list, then calls this same function on
 * the linked versions. At the en, link_list will contain all the links in the chain
 * @param v
 * @param up
 * @param link_list
 */
oi.getChain = function (v, up, link_list, include_hidden) {

    var links;

    if (include_hidden) {
        links = (up) ? v.all_up_links : v.all_down_links;
    } else {
        links = (up) ? v.up_links : v.down_links;
    }


    // check for loops
    links.forEach(function (l){
        if (link_list.indexOf(links[l]) == -1) {
            link_list.push(l);
            oi.getChain((up ? l.from : l.to), up, link_list, include_hidden);
        }
    });
};




oi.tooltipText = function (v) {

    info = {};
    info['<span>'+ v.group.name + ': v' + v.v+'</span>'] = v.date;

    html =  oi.tooltipDl(info, 'info') +
        oi.tooltipDl(v.params, 'params');

    if (typeof(v.colnames) !== 'undefined') {
        html += oi.colnames(v.colnames);
    }

    return(html);

};

/**
 * Returns a html description list of the params for the version
 * @param params
 */
oi.tooltipDl = function (params, class_name) {

    var html = '';
    Object.keys(params).forEach(function(p_name) {
        html += '<tr><td class="name">'+p_name+'</td><td class="val">'+params[p_name]+'</td></tr>'
    });
    html = '<table class="'+class_name+'">'+html+'</table>';
    return html;

};

oi.colnames = function (colnames) {
    var html;

    colnames = colnames.join(', ');
    html = '<div class="colnames"><span class="colnames_title">Cols</span><span class="colnames_content">';
    html += colnames;
    html += '</span></div>';
    return(html);


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





ff = {};

ff.init = function () {

    ff.el = oi.container.append('div')
        .attr('id', 'ff')
        .data([ {"x":0, "y":0} ]);

    ff.cutoff_date = null;

    ff.createGroupSelector();

    ff.el.call(d3.behavior.drag().on('drag', function (d,i) {
        console.log(d);
        d.x += d3.event.dx;
        d.y += d3.event.dy;
        d3.select(this).style("transform", "translate(" + d.x + "px," + d.y + "px)");
    }));

};


ff.createGroupSelector = function (group) {

    ff.group_selector = ff.el.append('div')
        .attr('id', 'group_selector')
        .append('select')
        .on('change', function (){
            ff.selected_group = this.options[this.selectedIndex].value
            ff.createDateSelector(ff.selected_group);
            oi.selectGroup(ff.selected_group, ff.cutoff_date)
        });
    ff.group_selector.append('option')
        .attr('val', '')
        .text('all');

    oi.groups.forEach(function (g) {
        ff.group_selector.append('option')
            .attr('val', g.name)
            .text(g.name);

    });

};

/**
 * Add a date range selector
 * Use a numeric slider to keep the interface quick to use and simple,
 * all of the dates are kept in an array, and the position of the slider
 * is the index of the cutoff date.
 * @param group
 */
ff.createDateSelector = function (group) {

    var groups, g;

    ff.cutoff_date = null;

    if (typeof(ff.ds) == 'object') {
        ff.ds.remove();
    }



    g = oi.getGroup(group);

    if (typeof(g) === 'undefined') {
        return;
    }

    // get dates for this selected group or for all groups
    // depending on whether a group name was passed.
    // if a group name was passed, construct an array length 1, so that
    // the same operation can be used to get a flat array of dates
    // - currently only doing this if a group is selected, but this way we can add in some
    //   action for changing the style of versions before selected date for all groups later
    groups = (typeof(g) !== 'undefined') ? [g] : oi.groups;
    ff.dates = [].concat.apply([], groups.map(function (g) { return g.all_versions.map(function (v) { return(v.date)}) }));

    ff.dates.sort();


    // initialise cutoff date to the oldest to show all
    ff.cutoff_date = ff.dates[0];



    if (ff.dates.length === 1) {
        return;
    }

    ff.ds = ff.el.append('div');
    ff.ds.append('p').node().innerHTML = 'Date range:'
    ff.ds.append('input')
        .attr('type','range')
        .attr('min', 0)
        .attr('max', ff.dates.length - 1)
        .attr('step', 1)
        .attr('value', 0)
        .on('change', function () {
            ff.setCutoffDate(ff.dates[this.value]);
        })
        .on('input', function (){
            ff.setCutoffDate(ff.dates[this.value]);
        });

    ff.ds.append('div').node().innerHTML =
        '<p>from: <span id="from_date"></span></p>' +
        '<p>to: <span id="to_date"></span></p>';

    ff.from_date = ff.el.select('#from_date');
    ff.ds.select('#from_date').node().innerHTML = ff.dateMsg(ff.cutoff_date);
    ff.ds.select('#to_date').node().innerHTML = ff.dateMsg(ff.dates[ff.dates.length - 1]);


};

/**
 * Sets the cutoff date. Changes the visible versions
 * updates the cutoff el
 * @param date string
 */
ff.setCutoffDate = function (date) {
    ff.cutoff_date = date;
    ff.from_date.node().innerHTML = ff.dateMsg(date);
    oi.selectGroup(ff.selected_group ,ff.cutoff_date);
};


ff.dateMsg = function (date) {
    var ms_ago = new Date() - new Date(date);
    var mins_ago = ms_ago / (1000 * 60);
    var hours_ago = mins_ago / 60;
    var days_ago = hours_ago / 24;
    if (days_ago > 2) {
        ago = Math.round(days_ago) + " days";
    } else if (hours_ago > 120) {
        ago = Math.round(hours_ago) + " hours";
    } else {
        ago = Math.round(mins_ago) + " mins";
    }
    return date + " (" + ago + " ago)";
}




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

util.zoom = function (el, by, x, y) {

    el.node().setAttribute("transform", "scale("+value+") translate("+ right +","+down+")");


};


util.shallowClone = function (obj) {

    // for i in obj might be faster
    return JSON.parse(JSON.stringify(obj));

}


/**
 * converts date to the number of days ago
 * @param date
 */
util.daysAgo = function (date) {

};
