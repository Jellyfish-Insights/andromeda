const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const merge = require('webpack-merge');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    const extractCSS = new ExtractTextPlugin('vendor.css');

    const sharedConfig = {
        stats: {
            modules: false
        },
        resolve: {
            extensions: ['.js']
        },
        module: {
            rules: [{
                test: /\.(png|woff|woff2|eot|ttf|svg)(\?|$)/,
                use: 'url-loader?limit=100000'
            }]
        },
        entry: {
            vendor: [
                'foundation-sites',
                'domain-task',
                'event-source-polyfill',
                'history',
                'react',
                'react-dom',
                'react-router-dom',
                'react-redux',
                'redux',
                'react-router-redux',
                'jquery',
                'moment',
                'react-autosuggest',
                'react-day-picker',
                'react-dnd',
                'react-dnd-html5-backend',
                'react-loading',
                'react-modal',
                'react-select',
                'react-tabs',
                'react-tooltip',
                'recharts',
                'underscore',
                'xlsx',
                'fuse.js',
                'immutability-helper',
                'lodash'
            ],
        },
        output: {
            publicPath: 'dist/',
            filename: '[name].js',
            library: '[name]_[hash]',
        },
        plugins: [
            new webpack.ProvidePlugin({
                $: 'jquery',
                jQuery: 'jquery',
                foundation: 'Foundation'
            }), // Maps identifiers
            new webpack.NormalModuleReplacementPlugin(/\/iconv-loader$/, require.resolve('node-noop')), // Workaround for https://github.com/andris9/encoding/issues/16
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"'
            })
        ]
    };

    const clientBundleConfig = merge(sharedConfig, {
        output: {
            path: path.join(__dirname, 'wwwroot', 'dist')
        },
        module: {
            rules: [{
                test: /\.css(\?|$)/,
                use: extractCSS.extract({
                    use: isDevBuild ? 'css-loader' : 'css-loader?minimize'
                })
            }]
        },
        plugins: [
            extractCSS,
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
        ].concat(isDevBuild ? [] : [
            new UglifyJSPlugin({
                uglifyOptions: {
                    ie8: false,
                    mangle: true,
                    compress: true
                }
            })
        ])
    });

    return [clientBundleConfig];
};
