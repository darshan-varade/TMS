$(function () {
    var dataEl = document.getElementById('dashboardChartData');
    if (!dataEl) return;

    var chartData;
    try {
        chartData = JSON.parse(dataEl.textContent);
    } catch (e) {
        return;
    }

    var statusCanvas = document.getElementById('statusChart');
    if (statusCanvas && chartData.status) {
        new Chart(statusCanvas, {
            type: 'doughnut',
            data: {
                labels: chartData.status.map(function (s) { return s.Label; }),
                datasets: [{
                    data: chartData.status.map(function (s) { return s.Value; }),
                    backgroundColor: ['#6366F1', '#8B5CF6', '#F59E0B', '#10B981', '#EF4444', '#64748B'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                cutout: '72%'
            }
        });
    }

    var priorityCanvas = document.getElementById('priorityChart');
    if (priorityCanvas && chartData.priority) {
        new Chart(priorityCanvas, {
            type: 'bar',
            data: {
                labels: chartData.priority.map(function (p) { return p.Label; }),
                datasets: [{
                    data: chartData.priority.map(function (p) { return p.Value; }),
                    backgroundColor: '#4f46e5',
                    borderRadius: 4,
                    maxBarThickness: 32
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#888', font: { size: 11 } }
                    },
                    y: {
                        grid: { color: 'rgba(0,0,0,0.05)' },
                        ticks: { stepSize: 1, color: '#888' }
                    }
                }
            }
        });
    }
});
