const instances = new Map();
let depsPromise;

async function ensureDeps() {
    if (!depsPromise) {
        depsPromise = (async () => {
            const [leaflet, mgrs] = await Promise.all([
                import('https://unpkg.com/leaflet@1.9.4/dist/leaflet-src.esm.js'),
                import('https://cdn.skypack.dev/mgrs')
            ]);

            injectLeafletCss();
            return { leaflet, mgrs };
        })();
    }

    return depsPromise;
}

function injectLeafletCss() {
    const existing = document.querySelector('link[data-leaflet]');
    if (existing) return;

    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = 'https://unpkg.com/leaflet@1.9.4/dist/leaflet.css';
    link.setAttribute('data-leaflet', 'true');
    document.head.appendChild(link);
}

function metersToLat(meters) {
    return meters / 111_320;
}

function metersToLon(meters, latitude) {
    return meters / (111_320 * Math.cos((latitude * Math.PI) / 180));
}

function buildGrid(bounds, mgrsLib) {
    const latStep = metersToLat(bounds.cellSizeMeters);
    const cells = [];
    let row = 0;

    for (let lat = bounds.maxLat; lat - latStep > bounds.minLat; lat -= latStep) {
        const nextLat = lat - latStep;
        const centerLat = (lat + nextLat) / 2;
        const lonStep = metersToLon(bounds.cellSizeMeters, centerLat);
        let column = 0;

        for (let lon = bounds.minLon; lon + lonStep < bounds.maxLon; lon += lonStep) {
            const nextLon = lon + lonStep;
            const centerLon = (lon + nextLon) / 2;
            const mgrsCode = mgrsLib.forward([centerLon, centerLat], 1);
            const zoneBand = mgrsCode.slice(0, 3);

            cells.push({
                bounds: [[nextLat, lon], [lat, nextLon]],
                center: [centerLat, centerLon],
                mgrs: mgrsCode,
                row,
                column,
                zoneBand
            });

            column += 1;
        }

        row += 1;
    }

    return cells;
}

function createLabelHtml(mgrsCode) {
    return `<div class="mgrs-label">${mgrsCode}</div>`;
}

function applyState(rect, ordered) {
    rect.setStyle({
        color: ordered ? '#166534' : '#2563eb',
        weight: ordered ? 1.5 : 1,
        fillColor: ordered ? '#a7f3d0' : '#bfdbfe',
        fillOpacity: ordered ? 0.6 : 0.25
    });
}

export const MgrsMap = {
    async init(elementId, dotnetRef, bounds, orderedCodes) {
        const { leaflet: L, mgrs } = await ensureDeps();
        const mapElement = document.getElementById(elementId);
        if (!mapElement) return;

        const map = L.map(mapElement, { zoomControl: true });
        map.fitBounds([[bounds.minLat, bounds.minLon], [bounds.maxLat, bounds.maxLon]]);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap-bidragsydere'
        }).addTo(map);

        const gridLayer = L.layerGroup().addTo(map);
        const labelLayer = L.layerGroup().addTo(map);

        const orderedSet = new Set(orderedCodes || []);
        const cells = buildGrid(bounds, mgrs);
        const state = { map, gridLayer, labelLayer, orderedSet, dotnetRef, selected: null, cells };

        cells.forEach(cell => {
            const rect = L.rectangle(cell.bounds, {
                color: '#2563eb',
                weight: 1,
                fillColor: '#bfdbfe',
                fillOpacity: 0.25,
                interactive: true
            }).addTo(gridLayer);

            if (orderedSet.has(cell.mgrs)) {
                applyState(rect, true);
            }

            rect.on('click', () => selectCell(cell, rect, state));
            cell.rect = rect;

            const label = L.marker(cell.center, {
                icon: L.divIcon({
                    className: 'mgrs-center-label',
                    html: createLabelHtml(cell.mgrs)
                }),
                interactive: false
            }).addTo(labelLayer);
            cell.label = label;
        });

        instances.set(elementId, state);
    },

    markOrdered(mgrsCode) {
        instances.forEach(state => {
            const cell = state.cells.find(c => c.mgrs === mgrsCode);
            if (!cell || !cell.rect) return;
            state.orderedSet.add(mgrsCode);
            applyState(cell.rect, true);
        });
    },

    dispose(elementId) {
        const state = instances.get(elementId);
        if (!state) return;
        state.map.remove();
        instances.delete(elementId);
    }
};

function selectCell(cell, rect, state) {
    if (state.selected?.rect) {
        applyState(state.selected.rect, state.orderedSet.has(state.selected.mgrs));
    }

    rect.setStyle({ color: '#2563eb', weight: 2.5, fillColor: '#2563eb', fillOpacity: 0.35 });
    state.selected = { ...cell, rect };

    if (state.dotnetRef) {
        state.dotnetRef.invokeMethodAsync('NotifyTileSelected', {
            mgrsCode: cell.mgrs,
            centerLat: cell.center[0],
            centerLon: cell.center[1],
            row: cell.row,
            column: cell.column,
            zoneBand: cell.zoneBand
        });
    }
}
