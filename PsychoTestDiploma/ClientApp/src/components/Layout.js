import React, { Component } from 'react';
import { Container } from 'reactstrap';
import { NavMenu } from './NavMenu';
import { Authorisation } from './Authorisation';
import { AdminsNavMenu } from './AdminsNavMenu';

export class Layout extends Component {
    static displayName = Layout.name;

    constructor(props) {
        super(props);
        this.state = {
            isLoggedIn: "",
            isAdmin: false
        };
    }

    componentDidMount() {
        this.getData();
        this.checkRole();
    }

    async getData() {
        const token = sessionStorage.getItem('tokenKey');
        const response = await fetch("/api/Authorization/getlogin", {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + token  // передача токена в заголовке
            }
        });
        if (response.ok === true) {
            {
                this.setState({ isLoggedIn: true });
            }
        }
        else {
            console.log("Status: ", response.status);
            this.setState({ isLoggedIn: false });
        }
    };

    async checkRole() {
        const token = sessionStorage.getItem('tokenKey');
        const response = await fetch("/api/Authorization/isAdmin", {
            method: "GET",
            headers: {
                "Accept": "application/json",
                "Authorization": "Bearer " + token
            }
        });
        if (response.ok === true) {
            {
                this.setState({ isAdmin: true });
            }
        }
        else {
            this.setState({ isAdmin: false });
        }
    };

    renderNav() {
        if (this.state.isAdmin === false)
            return (<NavMenu />);
        else return (<AdminsNavMenu />);
    }

    render() {
        if (this.state.isLoggedIn === false) {
            return (
                <div>
                    < Authorisation />
                </div>
            );
        }
        if (this.state.isLoggedIn === true) {
            return (
                <div>
                    <div>{this.renderNav()}</div>
                    <Container>
                        {this.props.children}
                    </Container>
                </div>
            );
        }
        if (this.state.isLoggedIn === "")
            return (<h2>Loading...</h2>);
    }
}

