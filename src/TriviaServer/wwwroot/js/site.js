// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

"use strict";
var chart;

var data = {
    labels: [],
    datasets: [{
        label: 'Score',
        data: [],
        backgroundColor: 'rgba(255, 99, 132, 0.2)',
        borderColor: 'rgba(255, 159, 64, 1)',
        borderWidth: 1
    }]
};

function initialize() {
    var ctx = document.getElementById('chart0').getContext('2d');
    chart = new Chart(ctx, {
        type: 'bar',
        data: data,
        options: {
            scales: {
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            },
            title: {
                fontSize: 18,
                display: true,
                text: "Trivia",
                position: "bottom"
            }
        }
    });
}

window.onload = function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/scoreHub").build();
    initialize();
    connection.start().then(function () {
    }).catch(function (err) {
        return console.error(err.toString());
    });
    connection.on("UpdateScore", function (req) {
        chart.data.labels = req.names;
        chart.data.datasets[0].data = req.scores;
        chart.update();
    });
}