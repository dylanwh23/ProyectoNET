// CarreraTiempoReal.razor.js - VERSIÓN MEJORADA CON FACTOR x5

var raceMap = null;
var runnerLayers = {}; 
var runnerState = {};  
var routePoints = [];  
var totalRouteLength = 0;
var animationFrameId = null;
var routeLayerGroup = null; 

const SIMULATION_FACTOR = 5.0; 

// Iconos
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
    iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
    iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
    shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

window.MapManager = {
    initMap: function (mapId) {
        var container = document.getElementById(mapId);
        if (container && container.clientHeight === 0) {
            container.style.height = "600px";
            container.style.display = "block";
        }

        if (raceMap) return;

        try {
            raceMap = L.map(mapId).setView([0, 0], 2);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { 
                attribution: '© OSM',
                maxZoom: 19
            }).addTo(raceMap);
            routeLayerGroup = L.layerGroup().addTo(raceMap);
            setTimeout(() => { if (raceMap) raceMap.invalidateSize(); }, 200);
            
            startAnimationLoop();
        } catch (e) { console.error("Error initMap:", e); }
    },

    updateRoute: function (geoJsonRoute) {
        if (!raceMap || !geoJsonRoute || geoJsonRoute.length < 10) return;

        try {
            if (routeLayerGroup) routeLayerGroup.clearLayers();
            routePoints = [];
            totalRouteLength = 0;

            const geoJsonObj = JSON.parse(geoJsonRoute);
            const routeLayer = L.geoJSON(geoJsonObj, {
                style: { color: '#007bff', weight: 5, opacity: 0.7 },
                pointToLayer: (f, l) => L.circleMarker(l, { radius: 3, color: 'orange' })
            });
            
            if (routeLayerGroup) routeLayerGroup.addLayer(routeLayer);
            processRouteGeometry(routeLayer);
            
            requestAnimationFrame(() => {
                if (raceMap) {
                    raceMap.invalidateSize();
                    const bounds = routeLayer.getBounds();
                    if (bounds.isValid()) raceMap.fitBounds(bounds, { padding: [50, 50] });
                }
            });
        } catch (e) { console.error("Error updateRoute:", e); }
    },

    updateRunnersBatch: function (runnersData) {
        if (!raceMap) return;
        const currentIds = new Set();
        
        runnersData.forEach(runner => {
            if (runner.lat === undefined || runner.lng === undefined) return;
            const id = runner.id;
            currentIds.add(id);

            const serverDist = (runner.km || 0) * 1000; 
            const serverSpeed = (runner.vel || 0) / 3.6; // m/s

            if (!runnerState[id]) {
                runnerState[id] = { 
                    currentDist: serverDist, 
                    speed: serverSpeed, 
                    rawLat: runner.lat, 
                    rawLng: runner.lng,
                    km: runner.km,
                    vel: runner.vel,
                    tiempoNeto: runner.tiempoNeto
                };
            } else {
                runnerState[id].speed = serverSpeed;
                runnerState[id].rawLat = runner.lat;
                runnerState[id].rawLng = runner.lng;
                runnerState[id].km = runner.km;
                runnerState[id].vel = runner.vel;
                runnerState[id].tiempoNeto = runner.tiempoNeto;

                // Lógica de sincronización: solo avanzar si servidor reporta progreso
                if (serverDist > runnerState[id].currentDist) {
                    if ((serverDist - runnerState[id].currentDist) > 500) {
                        runnerState[id].currentDist = serverDist;
                    }
                }
            }

            // Visual
            let color = "#0d6efd"; 
            let radius = 6;
            let zIndex = 100;
            if (runner.type === "leader") { color = "#ffc107"; radius = 10; zIndex = 300; }
            if (runner.type === "searched") { color = "#198754"; radius = 9; zIndex = 200; }

            if (!runnerLayers[id]) {
                let pos = getLatLngAtDistance(runnerState[id].currentDist);
                if (!isValidLatLng(pos)) pos = [runner.lat, runner.lng];

                if (isValidLatLng(pos)) {
                    runnerLayers[id] = L.circleMarker(pos, {
                        radius: radius, 
                        fillColor: color, 
                        color: "#fff", 
                        weight: 2, 
                        opacity: 1, 
                        fillOpacity: 0.9
                    }).addTo(raceMap);
                    
                    if(runnerLayers[id]._path) runnerLayers[id]._path.style.zIndex = zIndex;
                    
                    const tooltipContent = `<b>#${id}</b><br>⚡ ${runner.vel.toFixed(1)} km/h<br>🏃 ${runner.km.toFixed(2)}km<br>⏱ ${runner.tiempoNeto}`;
                    runnerLayers[id].bindTooltip(tooltipContent, { 
                        direction: "top", 
                        offset: [0, -10], 
                        permanent: false 
                    });
                }
            } else {
                runnerLayers[id].setStyle({ fillColor: color, radius: radius });
                const tooltipContent = `<b>#${id}</b><br>⚡ ${runner.vel.toFixed(1)} km/h<br>🏃 ${runner.km.toFixed(2)}km<br>⏱ ${runner.tiempoNeto}`;
                runnerLayers[id].getTooltip().setContent(tooltipContent);
            }
        });

        // Limpieza de corredores que ya no están visibles
        Object.keys(runnerLayers).forEach(key => {
            if (!currentIds.has(parseInt(key))) {
                if (raceMap.hasLayer(runnerLayers[key])) raceMap.removeLayer(runnerLayers[key]);
                delete runnerLayers[key];
                delete runnerState[key];
            }
        });
    }
};

// MOTOR DE ANIMACIÓN (CON FACTOR X5)
var lastFrameTime = 0;
function startAnimationLoop() {
    lastFrameTime = performance.now();
    animationLoop();
}

function animationLoop() {
    if (!raceMap) return;
    const now = performance.now();
    let dt = (now - lastFrameTime) / 1000;
    if (dt > 0.1) dt = 0.1; 
    lastFrameTime = now;

    const canAnimate = routePoints.length > 1 && totalRouteLength > 0;

    Object.keys(runnerState).forEach(id => {
        const runner = runnerState[id];
        const layer = runnerLayers[id];

        if (layer) {
            if (canAnimate && runner.speed > 0) {
                // Fórmula: Distancia = Velocidad * Tiempo * FACTOR_SIMULACION
                runner.currentDist += runner.speed * dt * SIMULATION_FACTOR;
                
                if (runner.currentDist > totalRouteLength) runner.currentDist = totalRouteLength;
                
                const newPos = getLatLngAtDistance(runner.currentDist);
                if (isValidLatLng(newPos)) {
                    layer.setLatLng(newPos);
                }
            } else if (!canAnimate) {
                layer.setLatLng([runner.rawLat, runner.rawLng]);
            }
        }
    });
    animationFrameId = requestAnimationFrame(animationLoop);
}

// GEOMETRÍA
function isValidLatLng(arr) {
    return Array.isArray(arr) && arr.length === 2 && !isNaN(arr[0]) && !isNaN(arr[1]);
}

function processRouteGeometry(geoJsonLayer) {
    routePoints = [];
    totalRouteLength = 0;
    try {
        geoJsonLayer.eachLayer(function (layer) {
            if (layer instanceof L.Polyline && !(layer instanceof L.CircleMarker)) {
                const latlngs = layer.getLatLngs();
                const flatLatLngs = flattenLatLngs(latlngs);
                for (let i = 0; i < flatLatLngs.length; i++) {
                    const pt = flatLatLngs[i];
                    let dist = 0;
                    if (i > 0) dist = flatLatLngs[i - 1].distanceTo(pt);
                    totalRouteLength += dist;
                    routePoints.push({ lat: pt.lat, lng: pt.lng, accumDist: totalRouteLength });
                }
            }
        });
    } catch (e) { console.warn("Error procesando geometría:", e); routePoints = []; }
}

function flattenLatLngs(arr) {
    let result = [];
    if (!Array.isArray(arr)) return result;
    if (arr.length > 0 && (arr[0] instanceof L.LatLng || typeof arr[0].lat === 'number')) return arr;
    arr.forEach(item => {
        if (item instanceof L.LatLng || (item.lat && item.lng)) result.push(item);
        else if (Array.isArray(item)) result = result.concat(flattenLatLngs(item));
    });
    return result;
}

function getLatLngAtDistance(targetDist) {
    if (!routePoints || routePoints.length < 2) return null;
    if (targetDist <= 0) return [routePoints[0].lat, routePoints[0].lng];
    if (targetDist >= totalRouteLength) {
        const last = routePoints[routePoints.length - 1];
        return [last.lat, last.lng];
    }
    for (let i = 0; i < routePoints.length - 1; i++) {
        const p1 = routePoints[i];
        const p2 = routePoints[i+1];
        if (targetDist >= p1.accumDist && targetDist <= p2.accumDist) {
            const segmentLen = p2.accumDist - p1.accumDist;
            if (segmentLen <= 0.001) return [p1.lat, p1.lng];
            const progress = (targetDist - p1.accumDist) / segmentLen; 
            return [
                p1.lat + (p2.lat - p1.lat) * progress, 
                p1.lng + (p2.lng - p1.lng) * progress
            ];
        }
    }
    return null;
}