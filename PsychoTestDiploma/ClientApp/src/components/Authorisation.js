import React, { Component } from 'react';
import { Alert, InputGroup, InputGroupAddon } from 'reactstrap';
import '../custom.css'

export class Authorisation extends Component {
    static displayName = Authorisation.name;

    constructor(props) {
        super(props);
        this.state = {
            email: "",
            password: "",
            alertVisible: false
        };

        this.onSubmit = this.onSubmit.bind(this);
        this.onEmailChange = this.onEmailChange.bind(this);
        this.onPasswordChange = this.onPasswordChange.bind(this);
        this.onChangeAlert = this.onChangeAlert.bind(this);
    }

    onEmailChange(e) {
        this.setState({ email: e.target.value });
    }
    onPasswordChange(e) {
        this.setState({ password: e.target.value });
    }
    onChangeAlert(value) {
        this.setState({ alertVisible: value });
    }

    // Вход
    onSubmit(e) {
        e.preventDefault();
        if (this.state.email && this.state.password) {
            this.getTokenAsync();
        }
    }

    async getTokenAsync() {
        var response = await fetch("/authentication", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json;'
            },
            body: JSON.stringify({ Login: this.state.email, Password: this.state.password })
        });
        var data = await response.json();
        if (response.ok === true) {
            // сохраняем в хранилище sessionStorage токен доступа
            sessionStorage.setItem('tokenKey', data.accessToken);
            window.location.reload();
        }
        else {
            console.log("Error: ", response.status, data.errorText);
            if (data.errorText === "Invalid username or password.")
                this.setState({ alertVisible: true });
        }
    }


    render() {
        return (
            <div>
                <div className="loginBody" >
                    <form onSubmit={this.onSubmit} className="form shadow-lg bg-white">
                        <Alert color="danger" isOpen={this.state.alertVisible} toggle={() => { this.onChangeAlert(false) }} fade={false}>Неверный логин или пароль!</Alert >
                        <h2>Вход</h2>
                        <br />
                        <div className="row mt-2">
                            <label className="control-label col-6">Email:</label>
                            <input type="email"
                                required
                                className="form-control col-6"
                                placeholder="example@gmail.com"
                                value={this.state.email}
                                onChange={this.onEmailChange} />
                        </div>
                        <div className="row mt-2">
                            <label className="control-label col-6">Пароль:</label>
                            <input type="password"
                                required
                                className="form-control col-6"
                                placeholder="Пароль"
                                value={this.state.password}
                                onChange={this.onPasswordChange} />
                        </div>
                        <br />
                        <div className="row mt-2">
                            <input className="btn btn-info col-12" type="submit" value="Войти" />
                        </div>
                    </form>
                </div>
                <br />
            </div>
        );
    }
}
