(A) Download React.js from CDN
-------------------------------
    1) https://cdnjs.cloudflare.com/ajax/libs/react/18.2.0/umd/react.production.min.js
    2) https://cdnjs.cloudflare.com/ajax/libs/react-dom/18.2.0/umd/react-dom.production.min.js
    3) https://cdnjs.cloudflare.com/ajax/libs/babel-standalone/7.22.5/babel.min.js
----------------------------------------------------------------------------------------------------------


(B) Add files into BundleConfig.cs
-----------------------------------
   
    1) bundles.Add(new Bundle("~/bundles/react").Include(
                      "~/Scripts/React/react.js",
                      "~/Scripts/React/react-dom.production.min.js",
                      "~/Scripts/React/babel.min.js"));

    2) bundles.Add(new Bundle("~/bundles/react_components").Include(
                      "~/Scripts/React/Components/About.js"));
----------------------------------------------------------------------------------------------------------


(C) HTML [RenderFormat]
------------------------
    
    <div id="root"></div> 
    
    @Scripts.Render("~/bundles/react")
    
    @Scripts.RenderFormat("<script data-plugins='transform-modules-umd' type='text/babel' data-presets='react' data-type='module' src='{0}' > </script>", "~/bundles/react_components")

----------------------------------------------------------------------------------------------------------

(D) Js Componets
-----------------
    const root = ReactDOM.createRoot(document.getElementById('root'));
    
    root.render(<>
    	<h1>a;ldfalsdfm</h1>
    </>)


