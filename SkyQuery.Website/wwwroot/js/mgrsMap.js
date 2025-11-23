const instances = new Map();
let mgrsLibPromise;

const TILE_SIZE = 256;
const MIN_ZOOM = 6;
const MAX_ZOOM = 12;
const INITIAL_ZOOM = 7.2;

function ensureMgrs() {
    if (!mgrsLibPromise) {
        mgrsLibPromise = import('https://cdn.skypack.dev/mgrs');
    }
    return mgrsLibPromise;
}

function project(lon, lat, zoom) {
    const sinLat = Math.sin((lat * Math.PI) / 180);
    const scale = TILE_SIZE * Math.pow(2, zoom);
    return {
        x: scale * ((lon + 180) / 360),
        y: scale * (0.5 - Math.log((1 + sinLat) / (1 - sinLat)) / (4 * Math.PI))
    };
}

function unproject(x, y, zoom) {
    const scale = TILE_SIZE * Math.pow(2, zoom);
    const lon = (x / scale) * 360 - 180;
    const n = Math.PI - (2 * Math.PI * y) / scale;
    const lat = (180 / Math.PI) * Math.atan(0.5 * (Math.exp(n) - Math.exp(-n)));
    return { lon, lat };
}

function toWebMercator(lon, lat) {
    const R = 6378137;
    const x = (lon * Math.PI / 180) * R;
    const y = Math.log(Math.tan(Math.PI / 4 + (lat * Math.PI / 180) / 2)) * R;
    return { x, y };
}

function fromWebMercator(x, y) {
    const R = 6378137;
    const lon = (x / R) * (180 / Math.PI);
    const lat = (Math.atan(Math.exp(y / R)) - Math.PI / 4) * (360 / Math.PI);
    return { lon, lat };
}

function buildGrid(bounds, cellSizeMeters, mgrsLib) {
    const minMeters = toWebMercator(bounds.minLon, bounds.minLat);
    const maxMeters = toWebMercator(bounds.maxLon, bounds.maxLat);

    const startX = Math.floor(minMeters.x / cellSizeMeters) * cellSizeMeters;
    const endX = Math.ceil(maxMeters.x / cellSizeMeters) * cellSizeMeters;
    const startY = Math.floor(minMeters.y / cellSizeMeters) * cellSizeMeters;
    const endY = Math.ceil(maxMeters.y / cellSizeMeters) * cellSizeMeters;

    const cells = [];
    let row = 0;
    for (let y = endY; y > startY; y -= cellSizeMeters) {
        const nextY = y - cellSizeMeters;
        let column = 0;
        for (let x = startX; x < endX; x += cellSizeMeters) {
            const nextX = x + cellSizeMeters;
            const center = fromWebMercator((x + nextX) / 2, (y + nextY) / 2);
            const mgrsCode = mgrsLib.forward([center.lon, center.lat], 5);
            const zoneBand = mgrsCode.slice(0, 3);

            const southWest = fromWebMercator(x, nextY);
            const northEast = fromWebMercator(nextX, y);

            cells.push({
                mgrs: mgrsCode,
                zoneBand,
                bounds: {
                    minLat: southWest.lat,
                    minLon: southWest.lon,
                    maxLat: northEast.lat,
                    maxLon: northEast.lon
                },
                center,
                row,
                column,
                ordered: false
            });
            column += 1;
        }
        row += 1;
    }

    return cells;
}

class CustomMgrsMap {
    constructor(element, bounds, dotnetRef, orderedCodes, mgrsLib) {
        this.element = element;
        this.bounds = bounds;
        this.dotnetRef = dotnetRef;
        this.mgrsLib = mgrsLib;
        this.ordered = new Set(orderedCodes || []);
        this.center = { lat: (bounds.minLat + bounds.maxLat) / 2, lon: (bounds.minLon + bounds.maxLon) / 2 };
        this.zoom = INITIAL_ZOOM;
        this.isPanning = false;
        this.lastPan = null;
        this.cells = buildGrid(bounds, bounds.cellSizeMeters, mgrsLib);
        this.handleResize = () => this.render();

        this.element.classList.add('mgrs-map-shell');
        this.tileLayer = document.createElement('div');
        this.tileLayer.className = 'mgrs-tiles';
        this.gridCanvas = document.createElement('canvas');
        this.gridCanvas.className = 'mgrs-grid';
        this.labelLayer = document.createElement('div');
        this.labelLayer.className = 'mgrs-label-layer';

        this.element.innerHTML = '';
        this.element.appendChild(this.tileLayer);
        this.element.appendChild(this.gridCanvas);
        this.element.appendChild(this.labelLayer);

        this.boundClick = (e) => this.handleClick(e);
        this.boundPanStart = (e) => this.handlePanStart(e);
        this.boundPanEnd = () => this.handlePanEnd();
        this.boundPanMove = (e) => this.handlePanMove(e);
        this.boundWheel = (e) => this.handleWheel(e);

        this.gridCanvas.addEventListener('click', this.boundClick);
        this.gridCanvas.addEventListener('mousedown', this.boundPanStart);
        window.addEventListener('mouseup', this.boundPanEnd);
        window.addEventListener('mousemove', this.boundPanMove);
        this.gridCanvas.addEventListener('wheel', this.boundWheel);
        window.addEventListener('resize', this.handleResize);

        this.render();
    }

    handleWheel(event) {
        event.preventDefault();
        const delta = Math.sign(event.deltaY);
        const newZoom = Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, this.zoom - delta * 0.25));
        if (newZoom === this.zoom) return;

        const mouseRect = this.gridCanvas.getBoundingClientRect();
        const offsetX = event.clientX - mouseRect.left - this.gridCanvas.width / 2;
        const offsetY = event.clientY - mouseRect.top - this.gridCanvas.height / 2;
        const centerPixels = project(this.center.lon, this.center.lat, this.zoom);
        const worldSizeCurrent = TILE_SIZE * Math.pow(2, this.zoom);
        const targetX = centerPixels.x + offsetX;
        const targetY = centerPixels.y + offsetY;
        const targetLatLon = unproject(targetX, targetY, this.zoom);

        this.zoom = newZoom;
        const newCenterPx = project(targetLatLon.lon, targetLatLon.lat, this.zoom);
        const worldSizeNew = TILE_SIZE * Math.pow(2, this.zoom);
        const dx = (offsetX / worldSizeCurrent) * worldSizeNew;
        const dy = (offsetY / worldSizeCurrent) * worldSizeNew;
        const adjusted = unproject(newCenterPx.x - dx, newCenterPx.y - dy, this.zoom);
        this.center = { lat: adjusted.lat, lon: adjusted.lon };
        this.render();
    }

    handlePanStart(event) {
        this.isPanning = true;
        this.lastPan = { x: event.clientX, y: event.clientY };
    }

    handlePanMove(event) {
        if (!this.isPanning || !this.lastPan) return;
        const dx = event.clientX - this.lastPan.x;
        const dy = event.clientY - this.lastPan.y;
        this.lastPan = { x: event.clientX, y: event.clientY };

        const scale = TILE_SIZE * Math.pow(2, this.zoom);
        const centerPx = project(this.center.lon, this.center.lat, this.zoom);
        const newCenterPx = { x: centerPx.x - dx, y: centerPx.y - dy };
        const newCenter = unproject(newCenterPx.x, newCenterPx.y, this.zoom);
        this.center = newCenter;
        this.render();
    }

    handlePanEnd() {
        this.isPanning = false;
        this.lastPan = null;
    }

    handleClick(event) {
        const rect = this.gridCanvas.getBoundingClientRect();
        const x = event.clientX - rect.left - this.gridCanvas.width / 2;
        const y = event.clientY - rect.top - this.gridCanvas.height / 2;
        const centerPx = project(this.center.lon, this.center.lat, this.zoom);
        const worldX = centerPx.x + x;
        const worldY = centerPx.y + y;
        const { lat, lon } = unproject(worldX, worldY, this.zoom);

        const hit = this.cells.find(c => lat <= c.bounds.maxLat && lat >= c.bounds.minLat && lon >= c.bounds.minLon && lon <= c.bounds.maxLon);
        if (hit && this.dotnetRef) {
            this.dotnetRef.invokeMethodAsync('NotifyTileSelected', {
                mgrsCode: hit.mgrs,
                centerLat: hit.center.lat,
                centerLon: hit.center.lon,
                row: hit.row,
                column: hit.column,
                zoneBand: hit.zoneBand
            });
            this.highlightSelection(hit);
        }
    }

    markOrdered(code) {
        this.ordered.add(code);
        this.render();
    }

    highlightSelection(cell) {
        this.selectedMgrs = cell.mgrs;
        this.render();
    }

    render() {
        const width = this.element.clientWidth;
        const height = this.element.clientHeight;
        this.gridCanvas.width = width;
        this.gridCanvas.height = height;
        this.drawTiles(width, height);
        this.drawGrid(width, height);
        this.drawLabels(width, height);
    }

    drawTiles(width, height) {
        const zoomInt = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, Math.round(this.zoom)));
        const scale = Math.pow(2, zoomInt);
        const centerPx = project(this.center.lon, this.center.lat, zoomInt);
        const halfW = width / 2;
        const halfH = height / 2;
        const minX = Math.floor((centerPx.x - halfW) / TILE_SIZE);
        const maxX = Math.floor((centerPx.x + halfW) / TILE_SIZE);
        const minY = Math.floor((centerPx.y - halfH) / TILE_SIZE);
        const maxY = Math.floor((centerPx.y + halfH) / TILE_SIZE);

        const needed = [];
        for (let x = minX; x <= maxX; x++) {
            for (let y = minY; y <= maxY; y++) {
                needed.push({ x, y });
            }
        }

        this.tileLayer.innerHTML = '';
        needed.forEach(tile => {
            const wrappedX = ((tile.x % scale) + scale) % scale;
            if (tile.y < 0 || tile.y >= scale) return;

            const img = document.createElement('img');
            img.loading = 'lazy';
            img.src = `https://tile.openstreetmap.org/${zoomInt}/${wrappedX}/${tile.y}.png`;
            img.className = 'mgrs-tile';
            img.style.left = `${tile.x * TILE_SIZE - centerPx.x + halfW}px`;
            img.style.top = `${tile.y * TILE_SIZE - centerPx.y + halfH}px`;
            img.width = TILE_SIZE;
            img.height = TILE_SIZE;
            this.tileLayer.appendChild(img);
        });
    }

    drawGrid(width, height) {
        const ctx = this.gridCanvas.getContext('2d');
        ctx.clearRect(0, 0, width, height);

        const zoomInt = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, Math.round(this.zoom)));
        const centerPx = project(this.center.lon, this.center.lat, zoomInt);
        const halfW = width / 2;
        const halfH = height / 2;

        this.cells.forEach(cell => {
            const topLeft = project(cell.bounds.minLon, cell.bounds.maxLat, zoomInt);
            const bottomRight = project(cell.bounds.maxLon, cell.bounds.minLat, zoomInt);
            const x = topLeft.x - centerPx.x + halfW;
            const y = topLeft.y - centerPx.y + halfH;
            const w = bottomRight.x - topLeft.x;
            const h = bottomRight.y - topLeft.y;

            const ordered = this.ordered.has(cell.mgrs);
            const isSelected = this.selectedMgrs === cell.mgrs;

            ctx.lineWidth = isSelected ? 2.2 : 1;
            ctx.strokeStyle = ordered ? '#166534' : '#2563eb';
            ctx.fillStyle = ordered ? 'rgba(22, 163, 74, 0.12)' : 'rgba(37, 99, 235, 0.12)';
            if (isSelected) {
                ctx.fillStyle = ordered ? 'rgba(22, 163, 74, 0.22)' : 'rgba(37, 99, 235, 0.22)';
            }
            ctx.fillRect(x, y, w, h);
            ctx.strokeRect(x, y, w, h);
        });
    }

    drawLabels(width, height) {
        this.labelLayer.innerHTML = '';
        const zoomInt = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, Math.round(this.zoom)));
        const centerPx = project(this.center.lon, this.center.lat, zoomInt);
        const halfW = width / 2;
        const halfH = height / 2;

        this.cells.forEach(cell => {
            const pos = project(cell.center.lon, cell.center.lat, zoomInt);
            const x = pos.x - centerPx.x + halfW;
            const y = pos.y - centerPx.y + halfH;
            if (x < -60 || x > width + 60 || y < -30 || y > height + 30) return;

            const tag = document.createElement('div');
            tag.className = 'mgrs-center-label';
            if (this.selectedMgrs === cell.mgrs) {
                tag.classList.add('selected');
            }
            tag.textContent = cell.mgrs;
            tag.style.left = `${x}px`;
            tag.style.top = `${y}px`;
            this.labelLayer.appendChild(tag);
        });
    }

    dispose() {
        window.removeEventListener('resize', this.handleResize);
        this.gridCanvas.removeEventListener('click', this.boundClick);
        this.gridCanvas.removeEventListener('mousedown', this.boundPanStart);
        window.removeEventListener('mouseup', this.boundPanEnd);
        window.removeEventListener('mousemove', this.boundPanMove);
        this.gridCanvas.removeEventListener('wheel', this.boundWheel);
    }
}

export const MgrsMap = {
    async init(elementId, dotnetRef, bounds, orderedCodes) {
        const mgrsLib = await ensureMgrs();
        const el = document.getElementById(elementId);
        if (!el) return;

        const instance = new CustomMgrsMap(el, bounds, dotnetRef, orderedCodes, mgrsLib);
        instances.set(elementId, instance);
    },

    markOrdered(mgrsCode) {
        instances.forEach(instance => instance.markOrdered(mgrsCode));
    },

    dispose(elementId) {
        const instance = instances.get(elementId);
        if (!instance) return;
        instance.dispose();
        instances.delete(elementId);
    }
};
