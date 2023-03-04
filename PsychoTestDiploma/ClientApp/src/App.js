import React, { Component } from 'react';
import './custom.css'
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Authorisation } from './components/Authorisation';
import { Patients } from './components/Patients';
import { Tests } from './components/Tests';
import Patient from './components/Patient';
import { Users } from './components/Users';

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Route exact path='/' component={Patients} />
                <Route path='/Login' component={Authorisation} />
                <Route path='/Tests' component={Tests} />
                <Route path='/Patient/:id' component={Patient} />
                <Route path='/Users' component={Users} />
            </Layout>
        );
    }
}
