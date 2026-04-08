// google-charts-interop.js
// Google Charts interop for Blazor

let charts = {};
let googleChartsLoaded = false;
let loadingPromise = null;

function normalizeDateColumns(data) {
    if (!Array.isArray(data) || data.length === 0) {
        return data;
    }

    const headers = data[0];
    if (!Array.isArray(headers)) {
        return data;
    }

    const dateColumnIndices = headers
        .map((header, index) => ({ header, index }))
        .filter(item => item.header && (item.header.type === 'date' || item.header.type === 'datetime'))
        .map(item => item.index);

    if (dateColumnIndices.length === 0) {
        return data;
    }

    return data.map((row, rowIndex) => {
        if (rowIndex === 0 || !Array.isArray(row)) {
            return row;
        }

        const normalizedRow = [...row];
        for (const columnIndex of dateColumnIndices) {
            const value = normalizedRow[columnIndex];
            if (value instanceof Date || value === null || value === undefined) {
                continue;
            }

            const parsedValue = new Date(value);
            if (!Number.isNaN(parsedValue.getTime())) {
                normalizedRow[columnIndex] = parsedValue;
            }
        }

        return normalizedRow;
    });
}

async function ensureGoogleChartsLoaded() {
    if (googleChartsLoaded) {
        return Promise.resolve();
    }

    if (loadingPromise) {
        return loadingPromise;
    }

    loadingPromise = new Promise((resolve, reject) => {
        if (typeof google !== 'undefined' && google.charts) {
            googleChartsLoaded = true;
            console.log('Google Charts already loaded');
            resolve();
            return;
        }

        console.log('Loading Google Charts...');
        const script = document.createElement('script');
        script.src = 'https://www.gstatic.com/charts/loader.js';
        script.onload = () => {
            console.log('Google Charts loader script loaded');
            google.charts.load('current', {
                packages: ['corechart'],
                language: 'fr'
            });
            google.charts.setOnLoadCallback(() => {
                googleChartsLoaded = true;
                console.log('Google Charts loaded successfully with French locale');
                resolve();
            });
        };
        script.onerror = () => {
            console.error('Failed to load Google Charts script');
            reject(new Error('Failed to load Google Charts'));
        };
        document.head.appendChild(script);
    });

    return loadingPromise;
}

function addMonthSeparators(chartId, dataTable) {
    try {
        console.log(`[addMonthSeparators] Starting for chart ${chartId}`);
        const container = document.getElementById(chartId);
        if (!container) {
            console.error(`[addMonthSeparators] Container ${chartId} not found`);
            return;
        }

        setTimeout(() => {
            console.log(`[addMonthSeparators] Looking for SVG in container ${chartId}`);
            const svg = container.querySelector('svg');
            if (!svg) {
                console.error(`[addMonthSeparators] SVG not found in container ${chartId}`);
                console.log('Container HTML:', container.innerHTML.substring(0, 200));
                return;
            }

            console.log(`[addMonthSeparators] SVG found, analyzing data for ${chartId}`);

            const monthStartIndices = [];
            let lastMonth = null;
            const rowCount = dataTable.getNumberOfRows();

            console.log(`[addMonthSeparators] Total rows: ${rowCount}`);

            for (let i = 0; i < rowCount; i++) {
                const label = dataTable.getValue(i, 0);
                if (!label) {
                    console.warn(`[addMonthSeparators] Empty label at row ${i}`);
                    continue;
                }

                let day = null;
                let month = null;

                if (label instanceof Date) {
                    day = label.getDate();
                    month = label.getMonth();
                } else {
                    const parts = String(label).split('/');
                    if (parts.length >= 2) {
                        day = parts[0].trim();
                        month = parts[1].trim();
                    }
                }

                if (day !== null && month !== null) {
                    if (i < 5) {
                        console.log(`[addMonthSeparators] Row ${i}: label="${label}", day="${day}", month="${month}"`);
                    }

                    if ((day === '01' || day === '1' || day === 1) && month !== lastMonth) {
                        monthStartIndices.push(i);
                        console.log(`[addMonthSeparators] ✓ Month start detected at index ${i}: ${label} (month changed from ${lastMonth} to ${month})`);
                    }
                    lastMonth = month;
                }
            }

            if (monthStartIndices.length === 0) {
                console.warn('[addMonthSeparators] No month starts detected. Check your data format.');
                console.log('Sample labels:', [0, 1, 2, 3, 4].map(i => i < rowCount ? dataTable.getValue(i, 0) : 'N/A'));
                return;
            }

            console.log(`[addMonthSeparators] Found ${monthStartIndices.length} month starts:`, monthStartIndices);

            const svgNS = "http://www.w3.org/2000/svg";
            const separatorGroup = document.createElementNS(svgNS, 'g');
            separatorGroup.setAttribute('class', 'month-separators');

            const rects = svg.querySelectorAll('rect');
            console.log(`[addMonthSeparators] Found ${rects.length} rectangles in SVG`);

            let chartArea = null;
            for (const rect of rects) {
                const width = parseFloat(rect.getAttribute('width'));
                const height = parseFloat(rect.getAttribute('height'));

                if (width > 100 && height > 100) {
                    chartArea = rect;
                    console.log(`[addMonthSeparators] Potential chart area: width=${width}, height=${height}`);
                    break;
                }
            }

            if (!chartArea) {
                console.error('[addMonthSeparators] Chart area not found');
                return;
            }

            const chartX = parseFloat(chartArea.getAttribute('x'));
            const chartY = parseFloat(chartArea.getAttribute('y'));
            const chartWidth = parseFloat(chartArea.getAttribute('width'));
            const chartHeight = parseFloat(chartArea.getAttribute('height'));

            console.log(`[addMonthSeparators] Chart area: x=${chartX}, y=${chartY}, width=${chartWidth}, height=${chartHeight}`);

            const totalDataPoints = rowCount;
            const xStep = chartWidth / (totalDataPoints - 1);

            console.log(`[addMonthSeparators] xStep=${xStep} (chartWidth=${chartWidth} / ${totalDataPoints - 1} points)`);

            monthStartIndices.forEach((index, idx) => {
                const xPos = chartX + (index * xStep);

                console.log(`[addMonthSeparators] Creating line ${idx + 1}/${monthStartIndices.length} at index ${index}, xPos=${xPos}`);

                const line = document.createElementNS(svgNS, 'line');
                line.setAttribute('x1', xPos);
                line.setAttribute('y1', chartY);
                line.setAttribute('x2', xPos);
                line.setAttribute('y2', chartY + chartHeight);
                line.setAttribute('stroke', '#FF0000');
                line.setAttribute('stroke-width', '4');
                line.setAttribute('stroke-dasharray', '0');
                line.setAttribute('opacity', '1');

                separatorGroup.appendChild(line);
            });

            svg.appendChild(separatorGroup);

            console.log(`[addMonthSeparators] ✅ Successfully added ${monthStartIndices.length} month separator lines to chart ${chartId}`);
        }, 500);
    } catch (error) {
        console.error(`[addMonthSeparators] Error for chart ${chartId}:`, error);
        console.error('Stack trace:', error.stack);
    }
}

async function initializeChart(chartId, data, options) {
    try {
        console.log(`Initializing chart ${chartId}...`, data, options);

        await ensureGoogleChartsLoaded();

        const container = document.getElementById(chartId);
        if (!container) {
            console.error(`Container with id ${chartId} not found`);
            throw new Error(`Container not found: ${chartId}`);
        }

        const chart = new google.visualization.ComboChart(container);
        const normalizedData = normalizeDateColumns(data);
        const dataTable = google.visualization.arrayToDataTable(normalizedData);

        chart.draw(dataTable, options);

        addMonthSeparators(chartId, dataTable);

        charts[chartId] = { chart, dataTable, options };

        console.log(`Chart ${chartId} initialized successfully`);
    } catch (error) {
        console.error(`Error initializing chart ${chartId}:`, error);
        throw error;
    }
}

async function updateChart(chartId, data, options) {
    try {
        console.log(`Updating chart ${chartId}...`);

        await ensureGoogleChartsLoaded();

        const chartInfo = charts[chartId];
        if (!chartInfo) {
            console.warn(`Chart ${chartId} not found, initializing...`);
            await initializeChart(chartId, data, options);
            return;
        }

        const container = document.getElementById(chartId);
        if (container) {
            const svg = container.querySelector('svg');
            if (svg) {
                const oldSeparators = svg.querySelector('.month-separators');
                if (oldSeparators) {
                    oldSeparators.remove();
                }
            }
        }

        const normalizedData = normalizeDateColumns(data);
        const dataTable = google.visualization.arrayToDataTable(normalizedData);

        chartInfo.chart.draw(dataTable, options);

        addMonthSeparators(chartId, dataTable);

        chartInfo.dataTable = dataTable;
        chartInfo.options = options;

        console.log(`Chart ${chartId} updated successfully`);
    } catch (error) {
        console.error(`Error updating chart ${chartId}:`, error);
        throw error;
    }
}

async function initializePieChart(chartId, data, options) {
    try {
        console.log(`Initializing pie chart ${chartId}...`, data, options);

        await ensureGoogleChartsLoaded();

        const container = document.getElementById(chartId);
        if (!container) {
            console.error(`Container with id ${chartId} not found`);
            throw new Error(`Container not found: ${chartId}`);
        }

        const chart = new google.visualization.PieChart(container);
        const normalizedData = normalizeDateColumns(data);
        const dataTable = google.visualization.arrayToDataTable(normalizedData);

        chart.draw(dataTable, options);

        charts[chartId] = { chart, dataTable, options };

        console.log(`Pie chart ${chartId} initialized successfully`);
    } catch (error) {
        console.error(`Error initializing pie chart ${chartId}:`, error);
        throw error;
    }
}

async function updatePieChart(chartId, data, options) {
    try {
        console.log(`Updating pie chart ${chartId}...`);

        await ensureGoogleChartsLoaded();

        const chartInfo = charts[chartId];
        if (!chartInfo) {
            console.warn(`Pie chart ${chartId} not found, initializing...`);
            await initializePieChart(chartId, data, options);
            return;
        }

        const normalizedData = normalizeDateColumns(data);
        const dataTable = google.visualization.arrayToDataTable(normalizedData);

        chartInfo.chart.draw(dataTable, options);

        chartInfo.dataTable = dataTable;
        chartInfo.options = options;

        console.log(`Pie chart ${chartId} updated successfully`);
    } catch (error) {
        console.error(`Error updating pie chart ${chartId}:`, error);
        throw error;
    }
}

function dispose(chartId) {
    try {
        const chartInfo = charts[chartId];
        if (chartInfo) {
            const container = document.getElementById(chartId);
            if (container) {
                container.innerHTML = '';
            }

            delete charts[chartId];
            console.log(`Chart ${chartId} disposed`);
        }
    } catch (error) {
        console.error(`Error disposing chart ${chartId}:`, error);
    }
}

async function exportChartAsImage(chartId, format = 'png') {
    try {
        const chartInfo = charts[chartId];
        if (!chartInfo) {
            throw new Error(`Chart ${chartId} not found`);
        }

        return chartInfo.chart.getImageURI();
    } catch (error) {
        console.error(`Error exporting chart ${chartId}:`, error);
        return null;
    }
}

function getSelection(chartId) {
    try {
        const chartInfo = charts[chartId];
        if (!chartInfo) {
            return null;
        }

        return chartInfo.chart.getSelection();
    } catch (error) {
        console.error(`Error getting selection from chart ${chartId}:`, error);
        return null;
    }
}

window.googleChartsInterop = {
    initializeChart,
    updateChart,
    initializePieChart,
    updatePieChart,
    dispose,
    exportChartAsImage,
    getSelection
};

async function initializeChart(chartId, data, options) {
    try {
        console.log(`Initializing chart ${chartId}...`, data, options);
      
  await ensureGoogleChartsLoaded();

 const container = document.getElementById(chartId);
  if (!container) {
            console.error(`Container with id ${chartId} not found`);
      throw new Error(`Container not found: ${chartId}`);
  }

        console.log(`Container found for ${chartId}`, container);

        const chart = new google.visualization.ComboChart(container);
        
        console.log(`Converting data for ${chartId}:`, data);
        const normalizedData = normalizeDateColumns(data);
        const dataTable = google.visualization.arrayToDataTable(normalizedData);
  console.log(`Data table created for ${chartId}`, dataTable);
      
    console.log(`Drawing chart ${chartId} with options:`, options);
        chart.draw(dataTable, options);
        
        addMonthSeparators(chartId, dataTable);
        
   charts[chartId] = { chart, dataTable, options };

        console.log(`Chart ${chartId} initialized successfully`);
    } catch (error) {
        console.error(`Error initializing chart ${chartId}:`, error);
   throw error;
    }
}

async function updateChart(chartId, data, options) {
    try {
        console.log(`Updating chart ${chartId}...`);
        
        await ensureGoogleChartsLoaded();

 const chartInfo = charts[chartId];
        if (!chartInfo) {
 console.warn(`Chart ${chartId} not found, initializing...`);
            await initializeChart(chartId, data, options);
            return;
}

        const container = document.getElementById(chartId);
        if (container) {
      const svg = container.querySelector('svg');
            if (svg) {
const oldSeparators = svg.querySelector('.month-separators');
if (oldSeparators) {
      oldSeparators.remove();
         }
   }
        }

        const normalizedData = normalizeDateColumns(data);
        const dataTable = google.visualization.arrayToDataTable(normalizedData);
      
        chartInfo.chart.draw(dataTable, options);
        
        addMonthSeparators(chartId, dataTable);
        
        chartInfo.dataTable = dataTable;
        chartInfo.options = options;

 console.log(`Chart ${chartId} updated successfully`);
    } catch (error) {
        console.error(`Error updating chart ${chartId}:`, error);
        throw error;
    }
}

function dispose(chartId) {
    try {
        const chartInfo = charts[chartId];
        if (chartInfo) {
      const container = document.getElementById(chartId);
            if (container) {
    container.innerHTML = '';
         }
          
            delete charts[chartId];
       console.log(`Chart ${chartId} disposed`);
        }
    } catch (error) {
    console.error(`Error disposing chart ${chartId}:`, error);
 }
}

async function exportChartAsImage(chartId, format = 'png') {
    try {
        const chartInfo = charts[chartId];
        if (!chartInfo) {
  throw new Error(`Chart ${chartId} not found`);
    }

        const imageUri = chartInfo.chart.getImageURI();
        return imageUri;
    } catch (error) {
     console.error(`Error exporting chart ${chartId}:`, error);
        return null;
    }
}

function getSelection(chartId) {
    try {
        const chartInfo = charts[chartId];
        if (!chartInfo) {
            return null;
        }

        const selection = chartInfo.chart.getSelection();
        return selection;
    } catch (error) {
        console.error(`Error getting selection from chart ${chartId}:`, error);
        return null;
    }
}

window.googleChartsInterop = {
    initializeChart,
    updateChart,
    initializePieChart,
    updatePieChart,
    dispose,
    exportChartAsImage,
    getSelection
};
