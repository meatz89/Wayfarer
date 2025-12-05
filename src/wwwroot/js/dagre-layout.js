// Dagre.js Layout Wrapper for Spawn Graph Visualization
// Uses Sugiyama algorithm for hierarchical DAG layout

window.DagreLayout = {
    computeLayout: function (graphData) {
        if (!graphData || !graphData.nodes || !graphData.edges) {
            console.error('DagreLayout: Invalid graph data');
            return { nodes: [], edges: [] };
        }

        var g = new dagre.graphlib.Graph();

        g.setGraph({
            rankdir: 'LR',
            nodesep: 80,
            ranksep: 200,
            edgesep: 40,
            marginx: 50,
            marginy: 50
        });

        g.setDefaultEdgeLabel(function () { return {}; });

        graphData.nodes.forEach(function (node) {
            var width = node.width || 180;
            var height = node.height || 80;

            if (node.type === 'scene') {
                width = 200;
                height = 100;
            } else if (node.type === 'situation') {
                width = 180;
                height = 80;
            } else if (node.type === 'choice') {
                width = 160;
                height = 70;
            } else if (node.type === 'entity') {
                width = 150;
                height = 60;
            }

            g.setNode(node.id, {
                label: node.label || node.id,
                width: width,
                height: height,
                type: node.type
            });
        });

        graphData.edges.forEach(function (edge) {
            g.setEdge(edge.source, edge.target);
        });

        dagre.layout(g);

        var layoutResult = {
            nodes: [],
            edges: [],
            graphWidth: g.graph().width || 800,
            graphHeight: g.graph().height || 600
        };

        g.nodes().forEach(function (nodeId) {
            var node = g.node(nodeId);
            if (node) {
                layoutResult.nodes.push({
                    id: nodeId,
                    x: node.x - (node.width / 2),
                    y: node.y - (node.height / 2),
                    width: node.width,
                    height: node.height
                });
            }
        });

        g.edges().forEach(function (edge) {
            var edgeData = g.edge(edge);
            if (edgeData && edgeData.points) {
                layoutResult.edges.push({
                    source: edge.v,
                    target: edge.w,
                    points: edgeData.points
                });
            }
        });

        return layoutResult;
    },

    addLinkLabels: function (containerId, linkMetadata) {
        if (!linkMetadata || linkMetadata.length === 0) {
            return;
        }

        var container = document.getElementById(containerId);
        if (!container) {
            console.error('DagreLayout.addLinkLabels: Container not found:', containerId);
            return;
        }

        var svg = container.querySelector('svg');
        if (!svg) {
            console.error('DagreLayout.addLinkLabels: SVG not found in container');
            return;
        }

        var existingLabels = svg.querySelectorAll('.link-label-group');
        existingLabels.forEach(function (label) {
            label.remove();
        });

        var linkPaths = svg.querySelectorAll('path');
        var linkPathArray = Array.from(linkPaths);

        linkMetadata.forEach(function (meta) {
            if (!meta.label) {
                return;
            }

            var matchedPath = DagreLayout.findMatchingPath(linkPathArray, meta.sourceX, meta.sourceY, meta.targetX, meta.targetY);
            if (!matchedPath) {
                return;
            }

            var pathLength = matchedPath.getTotalLength();
            var midPoint = matchedPath.getPointAtLength(pathLength / 2);

            var labelGroup = document.createElementNS('http://www.w3.org/2000/svg', 'g');
            labelGroup.setAttribute('class', 'link-label-group');
            labelGroup.setAttribute('transform', 'translate(' + midPoint.x + ',' + midPoint.y + ')');

            var textWidth = meta.label.length * 7 + 12;
            var rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
            rect.setAttribute('class', 'link-label-bg ' + (meta.cssClass || ''));
            rect.setAttribute('x', -textWidth / 2);
            rect.setAttribute('y', -10);
            rect.setAttribute('width', textWidth);
            rect.setAttribute('height', 20);
            rect.setAttribute('rx', 4);
            rect.setAttribute('ry', 4);
            rect.setAttribute('fill', meta.color || '#666');
            rect.setAttribute('fill-opacity', '0.9');

            var text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
            text.setAttribute('class', 'link-label-text');
            text.setAttribute('x', 0);
            text.setAttribute('y', 4);
            text.setAttribute('text-anchor', 'middle');
            text.setAttribute('dominant-baseline', 'middle');
            text.setAttribute('fill', 'white');
            text.setAttribute('font-size', '11px');
            text.setAttribute('font-weight', '500');
            text.textContent = meta.label;

            labelGroup.appendChild(rect);
            labelGroup.appendChild(text);
            svg.appendChild(labelGroup);
        });
    },

    findMatchingPath: function (pathArray, sourceX, sourceY, targetX, targetY) {
        var tolerance = 250;
        var bestMatch = null;
        var bestDistance = Infinity;

        for (var i = 0; i < pathArray.length; i++) {
            var path = pathArray[i];
            var pathLength = path.getTotalLength();
            if (pathLength === 0) continue;

            var startPoint = path.getPointAtLength(0);
            var endPoint = path.getPointAtLength(pathLength);

            var sourceDistance = Math.sqrt(
                Math.pow(startPoint.x - sourceX, 2) + Math.pow(startPoint.y - sourceY, 2)
            );
            var targetDistance = Math.sqrt(
                Math.pow(endPoint.x - targetX, 2) + Math.pow(endPoint.y - targetY, 2)
            );

            var totalDistance = sourceDistance + targetDistance;

            if (totalDistance < bestDistance && sourceDistance < tolerance && targetDistance < tolerance) {
                bestDistance = totalDistance;
                bestMatch = path;
            }
        }

        return bestMatch;
    }
};
