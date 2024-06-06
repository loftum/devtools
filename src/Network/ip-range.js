const uintMax = 4294967295

class IpRange {
    
    constructor(address, cidrBits) {
        this.address = address;
        this.cidrBits = cidrBits;
        
        let mask = 0;
        for (let ii=0; ii<cidrBits; ii++) {
            mask |= (1 << (31-ii));
        }
        this._min = address;
        this._max = this._min | (mask ^ uintMax); // uint.Max
        if (this._max < 0) {
            // bit overflow. Happens for 0.0.0.0/0
            this._max = uintMax - this._max - 1;
        }
    }
    
    /**
     * @returns {number}
     */
    get address() {
        return this._address;
    }

    set address(value) {
        this._address = value;
    }

    /**
     * @returns {number}
     */
    get cidrBits(){
        return this._cidrBits;
    }
    
    get min(){
        return this._min;
    }
    
    get max() {
        return this._max;
    }
    
    get size() {
        return this.max - this.min + 1;
    }

    set cidrBits(value) {
        this._cidrBits = value;
    }
    
    static parse(value) {
        let octets = [0, 0, 0, 0];
        let builder = "";

        let octetIndex = 0;
        let cidrIndex = 0;

        for (let ii=0; ii<value.length; ii++) {
            const c = value[ii];

            cidrIndex++;
            switch (c) {
                case '.':
                    if (octetIndex >= 3) {
                        return undefined;
                        //throw `Invalid CIDR '${value}'. Four octets is enough at ${cidrIndex}`;
                    }
                    octets[octetIndex] = parseInt(builder);
                    builder = "";
                    octetIndex++;
                    break;
                case '/':
                    if (octetIndex < 3) {
                        return undefined;
                        //throw `Invalid CIDR '${value}'. Expected more numbers at ${cidrIndex}`;
                    }

                    octets[octetIndex] = parseInt(builder);
                    builder = "";
                    octetIndex++;
                    break;
                default:
                    builder += c;
                    break;
            }
        }

        if (octetIndex !== 4 && builder.length > 0)
        {
            octets[octetIndex] = parseInt(builder);
            builder = "";
            octetIndex++;
        }

        if (octetIndex !== 4)
        {
            return undefined;
            //throw `Invalid CIDR '${value}'. Expected more numbers at ${cidrIndex}`;
        }
        const address = (octets[0] << 24) |
            (octets[1] << 16)|
            (octets[2] << 8) |
            (octets[3] << 0);
        return new IpRange(address, builder.length > 0 ? parseInt(builder) : 255);
    }
    
    split() {
        return this.splitInto(2);
    }
    
    splitInto(number) {
        if (number % 2 !== 0) {
            throw `Cannot split into ${number}. Must be positive and dividable by 2`;
        }

        const cidrBits = this._cidrBits + Math.log2(number);

        if (cidrBits > 32) {
            return undefined;
        }
        
        debugger;
        const chunk = this.size / number;
        let subRanges = [];
        for (let ii=0; ii< number; ii++) {
            subRanges.push(new IpRange(this.min + ii * chunk, cidrBits));
        }
        return subRanges;
    }
    
    format() {
        const address = this.address;
        const octets = [
            (address >> 24) & 0xff,
            (address >> 16) & 0xff,
            (address >> 8) & 0xff,
            (address & 0xff)
        ];
        
        const ip = octets.join('.');
        return `${ip}/${this.cidrBits}`;
    }
    
    info() {
        return `${this.format()} (${this.size})`
    }
}

class IpRangeElement extends HTMLElement {
    
    static observedAttributes = ["cidr"];
    
    constructor() {
        super();
        this.cidr = "";
        
        this.addEventListener("mouseover", () => this.style.backgroundColor = "lightblue");
        this.addEventListener("mouseout", () => this.style.backgroundColor = "");
    }
    
    set cidr(value) {
        this.range = IpRange.parse(value);
    }

    /**
     * @returns {string}
     */
    get cidr(){
        return this.range?.info() ?? "unknown";
    }

    /**
     * @returns {IpRange|undefined}
     */
    get range() {
        return this._range;
    }
    
    set range(range) {
        this._range = range;
    }
    
    get parent() {
        return this._parent;
    }
    
    set parent(parent) {
        this._parent = parent;
    }

    /**
     * @returns {string}
     */
    get direction() {
        const parent = this.parent;
        if (!parent){
            return "horizontal";
        }
        const parentValue = parent.direction; 
        switch(parentValue) {
            case "vertical":
                return "horizontal";
            case "horizontal":
                return "vertical";
            default:
                return "horizontal";
        }
    }
    
    click() {
        const container = this._container;
        if (container) {
            this.removeChild(container);
            this._container = undefined;
            return;
        }
        else {
            this.split();
        }
    }
    
    connectedCallback() {
        this.style.display = "flex";
        this.style.alignItems = "stretch";
        this.style.alignContent = "stretch";
        this.style.flexWrap = "nowrap";
        this.style.flexDirection = "column";
        this.style.margin = this.style.margin || "3px";
        this.style.boxSizing = "border-box";
        
        const titleBar = document.createElement("div");
        titleBar.style.display = "flex";
        titleBar.style.flexDirection = "row";
        titleBar.style.justifyContent = "space-between";
                
        this.appendChild(titleBar);
        
        const title = document.createElement("div");
        title.innerHTML = this.range?.info();
        titleBar.appendChild(title);
        titleBar.addEventListener("dblclick", this.click.bind(this));
        
        const button = document.createElement("button");
        button.style.cursor = "pointer";
        button.innerHTML = "/";
        button.addEventListener("click", this.click.bind(this));
        titleBar.append(button);
        
        const direction = this.direction;
        switch(direction) {
            case "horizontal":
                this.style.height = "100%";
                break;
            case "vertical":
                this.style.width = "100%";
        }
    }

    split() {
        
        if (this._container) {
            return;
        }

        const range = this._range;
        if (!range) {
            return;
        }
        const parts = range.split();

        if (!parts) {
            console.log("Could not split");
            return;
        }
        const [range1, range2] = parts;
        

        const container = document.createElement("div");
        this.appendChild(container);
        container.style.display = "flex";
        container.style.alignItems = "stretch";
        container.style.alignContent = "stretch";
        container.style.flexWrap = "nowrap";
        container.style.minHeight = "100%";
        container.style.minWidth = "100%";
        const direction = this.direction;
        switch(direction) {
            case "horizontal":
                container.style.flexDirection = "row";
                break;
            case "vertical":
                container.style.flexDirection = "column";
        }

        const sub1 = new IpRangeElement();
        sub1.parent = this;
        sub1.range = range1;
        container.appendChild(sub1);
        
        const sub2 = new IpRangeElement();
        sub2.parent = this;
        
        sub2.range = range2;
        container.appendChild(sub2);
        this._container = container;
    }
    
    disconnectedCallback() {
        
    }
    
    attributeChangedCallback(name, oldValue, newValue) {
        switch(name) {
            case "cidr":
                this.cidr = newValue;
        }
    }
}

customElements.define('ip-range', IpRangeElement);