const path = require('path')

const config = {
  mode: process.env.NODE_ENV === 'production' ? 'production' : 'development',
  entry: {
    site: './src/js/app.js'
  },
  output: {
    path: path.join(__dirname, './wwwroot/js'),
    filename: '[name].js',
    clean: true
  },
  devtool: process.env.NODE_ENV === 'production' ? 'source-map' : 'eval-source-map',
  module: {
    rules: [
      { 
        test: /\.(js|jsx)$/, 
        include: path.join(__dirname, 'src/js'), 
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env']
          }
        }
      }
    ]
  },
  externals: {
    jquery: 'jQuery'
  }
}

module.exports = config
