const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const merge = require('webpack-merge');
require('sass-loader');
require('css-loader');
require('css-hot-loader');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);
    const extractSass = new ExtractTextPlugin({
        filename: "[name].[contenthash].css",
        disable: isDevBuild
    });

    // Configuration in common to both client-side and server-side bundles
    const sharedConfig = () => ({
        stats: {
            modules: false
        },
        resolve: {
            extensions: ['.js', '.jsx', '.ts', '.tsx', '.scss', '.svg']
        },
        output: {
            filename: '[name].js',
            publicPath: 'dist/' // Webpack dev middleware, if enabled, handles requests for this URL prefix
        },
        module: {
            rules: [{
                    test: /\.tsx?$/,
                    include: /ClientApp/,
                    use: 'awesome-typescript-loader?silent=true'
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg)$/,
                    use: 'url-loader?limit=25000'
                },
                {
                    test: /\.s?css$/,
                    use: ['css-hot-loader'].concat(ExtractTextPlugin.extract({
                        fallback: 'style-loader',
                        use: isDevBuild ? 'css-loader!sass-loader' : 'css-loader?minimize!sass-loader'
                    }))
                }
            ]
        },
        plugins: [extractSass, new CheckerPlugin()]
    });

    // Configuration for client-side bundle suitable for running in browsers
    const clientBundleOutputDir = './wwwroot/dist';
    const clientBundleConfig = merge(sharedConfig(), {
        entry: {
            'main-client': './ClientApp/boot-client.tsx'
        },
        module: {
            rules: [{
                test: /\.css$/,
                use: ExtractTextPlugin.extract({
                    use: isDevBuild ? 'css-loader' : 'css-loader?minimize'
                })
            }]
        },
        output: {
            path: path.join(__dirname, clientBundleOutputDir)
        },
        plugins: [
            new ExtractTextPlugin('site.css'),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            })
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(clientBundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [
            // Plugins that apply in production builds only
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
